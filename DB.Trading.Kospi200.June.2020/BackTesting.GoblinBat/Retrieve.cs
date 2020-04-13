﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using ShareInvest.Catalog;
using ShareInvest.Catalog.XingAPI;
using ShareInvest.GoblinBatContext;

namespace ShareInvest.Strategy
{
    public partial class Retrieve : CallUpStatisticalAnalysis
    {
        public long OnReceiveRepositoryID(Catalog.XingAPI.Specify[] specifies) => GetRepositoryID(specifies);
        public Dictionary<DateTime, string> OnReceiveInformation(long number) => GetInformation(number);
        public Catalog.XingAPI.Specify[] OnReceiveStrategy(long index) => GetStrategy(index);
        public Retrieve(string key) : base(key) => Console.WriteLine(key);
        public void SetInitialzeTheCode(string code)
        {
            if (Chart == null && Quotes == null)
            {
                Chart = GetChart(code);
                Quotes = GetQuotes(code);
            }
        }
        public List<long> SetInitialzeTheCode()
        {
            Code = GetStrategy();
            SetInitialzeTheCode(Code);

            return GetStrategy("16.2");
        }
        public void SetInitializeTheChart()
        {
            if (Chart != null)
            {
                Chart.Clear();
                Chart = null;
            }
            if (Quotes != null)
            {
                Quotes.Clear();
                Quotes = null;
            }
        }
        public bool GetDuplicateResults(long index)
        {
            var now = DateTime.Now;

            if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 5)
                now = now.AddDays(-1);

            return GetDuplicateResults(index, now.ToString(date));
        }
        public string GetDate(string code)
        {
            if (DateTime.TryParseExact(SetDate(code).Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date))
                return string.Concat(date.ToLongDateString(), " ", date.ToShortTimeString());

            return string.Empty;
        }
        protected override string GetConvertCode(string code)
        {
            if (Code.Substring(0, 3).Equals(code.Substring(0, 3)) && Code.Substring(5).Equals(code.Substring(3)))
                return Code;

            return code;
        }
        public static string Code
        {
            get; set;
        }
        public static string Date
        {
            get
            {
                if (DateTime.TryParseExact(Quotes.Last().Time.Substring(0, 12), format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date))
                    return string.Concat(date.ToLongDateString(), " ", date.ToShortTimeString());

                else
                    return string.Empty;
            }
        }
        protected internal static Queue<Chart> Chart
        {
            get; private set;
        }
        protected internal static Queue<Quotes> Quotes
        {
            get; private set;
        }
        const string date = "yyMMdd";
        const string format = "yyMMddHHmmss";
    }
}