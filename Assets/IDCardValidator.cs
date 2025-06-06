using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DefaultNamespace
{
    public class IDCardValidator
    {
        
        
        /// <summary>
        /// 验证中国大陆身份证号码合法性
        /// </summary>
        public static bool IsValidIDCard(string idNumber)
        {
            try
            {
                // 1. 去除空格和特殊字符
                idNumber = idNumber.Trim().ToUpper();
                
                // 2. 长度验证
                if(idNumber.Length != 15 && idNumber.Length != 18)
                    return false;
                
                // 3. 模式验证
                if(idNumber.Length == 15)
                {
                    // 15位：1-6地区码，7-12出生日期（YYMMDD），13-15顺序码
                    if(!Regex.IsMatch(idNumber, @"^[1-9]\d{7}((0[1-9])|(1[0-2]))((0[1-9])|([1-2][0-9])|(3[0-1]))\d{3}$"))
                        return false;
                }
                else if(idNumber.Length == 18)
                {
                    // 18位：1-6地区码，7-14出生日期（YYYYMMDD），15-17顺序码，18校验码
                    if(!Regex.IsMatch(idNumber, @"^[1-9]\d{5}(18|19|20)\d{2}((0[1-9])|(1[0-2]))((0[1-9])|([1-2][0-9])|(3[0-1]))\d{3}[0-9X]$"))
                        return false;
                }
                
                
                // 4. 出生日期验证
               
                if(!CheckBirthDate(idNumber))
                    return false;
                
                
                // 5. 校验码验证（仅18位）
                if(idNumber.Length == 18)
                {
                    if(!VerifyChecksum(idNumber))
                        return false;
                }
                
                // 6. 地区码初步验证（前6位）
                if(!ValidateRegionCode(idNumber.Substring(0, 6)))
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证出生日期
        /// </summary>
        private static bool CheckBirthDate(string idNumber)
        {
            string dateStr;
            if(idNumber.Length == 15)
            {
                // 15位：YYMMDD + 19前缀
                dateStr = "19" + idNumber.Substring(6, 6);
            }
            else
            {
                // 18位：YYYYMMDD
                dateStr = idNumber.Substring(6, 8);
            }
            
            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));
            
            // 基本范围检查
            if(year < 1900 || year > DateTime.Today.Year) return false;
            if(month < 1 || month > 12) return false;
            if(day < 1 || day > 31) return false;
            
            // 月份具体天数验证
            if(month == 2)
            {
                // 闰年检查
                bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                return day <= (isLeapYear ? 29 : 28);
            }
            
            // 30天的月份
            if(new List<int> {4, 6, 9, 11}.Contains(month))
            {
                return day <= 30;
            }
            
            return true;
        }
        
        /// <summary>
        /// 18位身份证校验码验证
        /// </summary>
        private static bool VerifyChecksum(string idNumber)
        {
            int[] weights = {7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2};
            char[] checkChars = {'1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'};
            
            int sum = 0;
            for(int i = 0; i < 17; i++)
            {
                sum += int.Parse(idNumber[i].ToString()) * weights[i];
            }
            
            int mod = sum % 11;
            char expectedCheckChar = checkChars[mod];
            
            return idNumber[17] == expectedCheckChar;
        }
        
        /// <summary>
        /// 地区码初步验证（前6位）
        /// </summary>
        private static bool ValidateRegionCode(string regionCode)
        {
            // 实际项目中应该使用完整的行政区划代码表
            // 这里仅做简单验证：前2位表示省份，应为01-99之间的数字
            
            if(regionCode.Length != 6) return false;
            
            int provinceCode;
            if(!int.TryParse(regionCode.Substring(0, 2), out provinceCode))
                return false;
            
            return provinceCode >= 1 && provinceCode <= 99;
        }
        
        
        /// <summary>根据身份证号检查是否年满18岁</summary>
        public static bool IsAdult(string idCard)
        {
            // 1. 基础验证
            if (string.IsNullOrWhiteSpace(idCard))
            {
                Debug.LogError("身份证号不能为空");
                return false;
            }
            
            // 移除所有空格
            idCard = idCard.Trim();
            
            // 2. 验证身份证长度
            if (idCard.Length != 15 && idCard.Length != 18)
            {
                Debug.LogError("身份证号长度无效，应为15或18位");
                return false;
            }
            
            try
            {
                // 3. 提取出生日期
                string birthDateStr;
                if (idCard.Length == 15)  // 一代身份证
                {
                    birthDateStr = "19" + idCard.Substring(6, 6); // 19YYMMDD
                }
                else // 二代身份证
                {
                    birthDateStr = idCard.Substring(6, 8); // YYYYMMDD
                }
                
                // 4. 解析出生日期
                var birthYear = int.Parse(birthDateStr.Substring(0, 4));
                var birthMonth = int.Parse(birthDateStr.Substring(4, 2));
                var birthDay = int.Parse(birthDateStr.Substring(6, 2));
                
                // 5. 验证日期有效性
                if (!IsValidDate(birthYear, birthMonth, birthDay))
                {
                    Debug.LogError($"无效的出生日期: {birthYear}/{birthMonth}/{birthDay}");
                    return false;
                }
                
                // 6. 计算当前日期和18岁生日
                var birthDate = new DateTime(birthYear, birthMonth, birthDay);
                var eighteenYearsAgo = DateTime.Today.AddYears(-18);
                
                // 7. 比较日期（是否已过18岁生日）
                return birthDate <= eighteenYearsAgo;
            }
            catch (Exception ex)
            {
                Debug.LogError($"身份证解析错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 验证日期是否有效
        /// </summary>
        private static bool IsValidDate(int year, int month, int day)
        {
            // 基本范围检查
            if (year < 1900 || year > DateTime.Today.Year) return false;
            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;
            
            // 特定月份天数验证
            if (month == 2)
            {
                // 闰年检查
                bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
                return day <= (isLeapYear ? 29 : 28);
            }
            
            // 30天的月份
            if (new List<int> {4, 6, 9, 11}.Contains(month))
            {
                return day <= 30;
            }
            
            return true;
        }
    }
}