﻿using System;
using System.Collections;
using Microsoft.Win32;

namespace ShareInvest.Verify
{
    public enum DigitalProductIdVersion
    {
        UpToWindows7,
        Windows8AndUp
    }
    public static class KeyDecoder
    {
        public static string GetWindowsProductKeyFromRegistry()
        {
            var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            var registryKeyValue = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")?.GetValue("DigitalProductId");

            if (registryKeyValue != null)
            {
                var digitalProductId = (byte[])registryKeyValue;
                var isWin8OrUp = Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2 || Environment.OSVersion.Version.Major > 6;
                localKey.Close();

                return GetWindowsProductKeyFromDigitalProductId(digitalProductId, isWin8OrUp ? DigitalProductIdVersion.Windows8AndUp : DigitalProductIdVersion.UpToWindows7);
            }
            return string.Empty;
        }
        public static string GetWindowsProductKeyFromDigitalProductId(byte[] digitalProductId, DigitalProductIdVersion digitalProductIdVersion)
        {
            return digitalProductIdVersion == DigitalProductIdVersion.Windows8AndUp ? DecodeProductKeyWin8AndUp(digitalProductId) : DecodeProductKey(digitalProductId);
        }
        private static string DecodeProductKey(byte[] digitalProductId)
        {
            var digits = new[]
            {
                'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'P', 'Q', 'R', 'T', 'V', 'W', 'X', 'Y', '2', '3', '4', '6', '7', '8', '9'
            };
            var decodedChars = new char[decodeLength];
            var hexPid = new ArrayList();

            for (var i = keyStartIndex; i <= keyEndIndex; i++)
                hexPid.Add(digitalProductId[i]);

            for (var i = decodeLength - 1; i >= 0; i--)
            {
                if ((i + 1) % 6 == 0)
                    decodedChars[i] = '-';

                else
                {
                    var digitMapIndex = 0;

                    for (var j = decodeStringLength - 1; j >= 0; j--)
                    {
                        var byteValue = (digitMapIndex << 8) | (byte)hexPid[j];
                        hexPid[j] = (byte)(byteValue / 24);
                        digitMapIndex = byteValue % 24;
                        decodedChars[i] = digits[digitMapIndex];
                    }
                }
            }
            return new string(decodedChars);
        }
        public static string DecodeProductKeyWin8AndUp(byte[] digitalProductId)
        {
            var key = string.Empty;
            var isWin8 = (byte)((digitalProductId[66] / 6) & 1);
            var last = 0;
            digitalProductId[66] = (byte)((digitalProductId[66] & 0xf7) | (isWin8 & 2) * 4);

            for (var i = 24; i >= 0; i--)
            {
                var current = 0;

                for (var j = 14; j >= 0; j--)
                {
                    current *= 256;
                    current = digitalProductId[j + keyOffset] + current;
                    digitalProductId[j + keyOffset] = (byte)(current / 24);
                    current %= 24;
                    last = current;
                }
                key = digits[current] + key;
            }
            var keypart1 = key.Substring(1, last);
            var keypart2 = key.Substring(last + 1, key.Length - (last + 1));
            key = keypart1 + "N" + keypart2;

            for (var i = 5; i < key.Length; i += 6)
                key = key.Insert(i, "-");

            return key;
        }
        private const int decodeLength = 29;
        private const int decodeStringLength = 15;
        private const int keyOffset = 52;
        private const int keyStartIndex = 52;
        private const int keyEndIndex = keyStartIndex + 15;
        private const string digits = "BCDFGHJKMPQRTVWXY2346789";
    }
}