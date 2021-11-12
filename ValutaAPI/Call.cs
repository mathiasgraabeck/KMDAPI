﻿using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data;

namespace ValutaAPI
{
    public class Call
    {
        // reads all from ValutaKurser
        // takes a connection string
        public static ValutaKurs[] ReadSQL(string connectionString)
        {
            List<ValutaKurs> queryRes = new List<ValutaKurs>();

            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            string query = "select * from ValutaKurser";
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                queryRes.Add(new ValutaKurs(dr.GetInt32(0), dr.GetString(1), dr.GetString(2), dr.GetDateTime(3), dr.GetDecimal(4)));
            }

            con.Close();
            return queryRes.ToArray();
        }

        // reads from https://valutakurser.azurewebsites.net/ValutaKurs and returns the result in a ValutaKurs array

        public static ValutaKurs[] WebAPI()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://valutakurser.azurewebsites.net/ValutaKurs");

            HttpWebResponse httpResponse = (HttpWebResponse)webRequest.GetResponse();
            using StreamReader responseReader = new StreamReader(httpResponse.GetResponseStream());
            string responseString = responseReader.ReadToEnd();

            JObject rss = JObject.Parse(responseString);
            DateTime updatedAt = (DateTime)rss["updatedAt"];

            IJEnumerable<JToken> json = JObject.Parse(responseString).Children().Children().Children();

            JArray toCurrencies = new JArray(json.Values("toCurrency"));
            JArray rates = new JArray(json.Values("rate"));

            //saml JArrays i ValutaKurs array
            ValutaKurs[] valutaKurserArr = new ValutaKurs[rates.Count];

            for (int i = 0; i < rates.Count; i++)
            {
                valutaKurserArr[i] = new ValutaKurs(null, "DKK", (string)toCurrencies[i], updatedAt, (decimal)rates[i]);
            }
           
            return valutaKurserArr;
        }

        
        public static void WriteSQL(ValutaKurs[] valutaKurserArr, string connectionString)
        {
            SqlConnection con = new SqlConnection(connectionString);

            string query = "INSERT INTO ValutaKurser (FromCurrency,ToCurrency,UpdatedAt,Rate) VALUES (@FromCurrency,@ToCurrency,@UpdatedAt,@Rate)";

            using SqlCommand command = new SqlCommand(query, con);

            for (int i = 0; i < valutaKurserArr.Length; i++)
            {
                command.Parameters.AddWithValue("@FromCurrency ", valutaKurserArr[i].FromCurrency);
                command.Parameters.AddWithValue("@ToCurrency ", valutaKurserArr[i].ToCurrency);
                command.Parameters.AddWithValue("@UpdatedAt ", SqlDbType.DateTime2).Value = valutaKurserArr[i].UpdatedAt;
                command.Parameters.AddWithValue("@Rate  ", valutaKurserArr[i].Rate);
            }

            con.Open();
            int result = command.ExecuteNonQuery();

            // Check Error
            if (result < 0)
                Console.WriteLine("Error inserting data into Database!");
        }
    }
}