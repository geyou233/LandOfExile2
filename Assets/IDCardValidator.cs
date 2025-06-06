using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DefaultNamespace
{
    public class IDCardValidator
    {

        #region 验证姓名
        
        // [Header("验证设置")]
        // [Tooltip("最小姓名长度")]
        public static int minLength = 2;
    
        // [Tooltip("最大姓名长度")]
        public static int maxLength = 15;
    
        // [Tooltip("允许复姓")]
        public static bool allowCompoundSurnames = true;
    
        // [Tooltip("允许少数民族姓名中的点号")]
        public static bool allowEthnicDots = true;
    
        // [Header("高级设置")]
        // [Tooltip("启用高频姓氏检测")]
        public static bool checkCommonSurnames = true;
    
        // [Tooltip("启用罕见姓名检测")]
        public static bool checkRareNames = true;
        
        // [Tooltip("启用非法姓名模式检测")]
        public static bool checkInvalidPatterns = true;
        
        // [Tooltip("启用少数民族姓名检测")]
        public static bool checkEthnicNames = true;
        
        // [Header("地区设置")]
        public static bool mainlandChina = true;
        public static bool taiwan = false;
        public static bool hongKongMacau = false;
        public static bool international = false;
    
        // 中国常见姓氏列表（前100个）
        private static readonly List<string> commonSurnames = new List<string>() {
            "王", "李", "张", "刘", "陈", "杨", "赵", "黄", "周", "吴",
            "徐", "孙", "胡", "朱", "高", "林", "何", "郭", "马", "罗",
            "梁", "宋", "郑", "谢", "韩", "唐", "冯", "于", "董", "萧",
            "程", "曹", "袁", "邓", "许", "傅", "沈", "曾", "彭", "吕",
            "苏", "卢", "蒋", "蔡", "贾", "丁", "魏", "薛", "叶", "阎",
            "余", "潘", "杜", "戴", "夏", "钟", "汪", "田", "任", "姜",
            "范", "方", "石", "姚", "谭", "廖", "邹", "熊", "金", "陆",
            "郝", "孔", "白", "崔", "康", "毛", "邱", "秦", "江", "史",
            "顾", "侯", "邵", "孟", "龙", "万", "段", "雷", "钱", "汤",
            "尹", "黎", "易", "常", "武", "乔", "贺", "赖", "龚", "文"
        };
    
        // 常见复姓
        private static readonly List<string> compoundSurnames = new List<string>(){
            "欧阳", "上官", "皇甫", "司徒", "诸葛", 
            "司马", "宇文", "令狐", "慕容", "尉迟",
            "东方", "赫连", "澹台", "公冶", "宗政",
            "濮阳", "淳于", "单于", "太史", "申屠"
        };
    
        // 常见少数民族前缀
        private static readonly List<string> ethnicPrefixes = new List<string>(){
            "阿", "艾", "布", "才", "次", "达", "德", "狄", "俄", "额",
            "尔", "噶", "嘎", "格", "古", "果", "哈", "合", "赫", "呼",
            "吉", "加", "金", "喀", "卡", "克", "库", "奎", "拉", "腊",
            "兰", "朗", "勒", "雷", "李", "列", "卢", "鲁", "陆", "罗",
            "洛", "玛", "马", "麦", "满", "毛", "孟", "米", "苗", "莫",
            "木", "穆", "纳", "南", "尼", "倪", "聂", "宁", "牛", "农",
            "诺", "欧", "帕", "潘", "庞", "裴", "彭", "普", "其", "奇",
            "恰", "钱", "强", "乔", "秦", "邱", "曲", "全", "仁", "荣",
            "茹", "阮", "桑", "沙", "山", "单", "尚", "邵", "舍", "沈",
            "盛", "石", "时", "史", "舒", "司", "松", "宋", "苏", "孙",
            "索", "塔", "邰", "谭", "汤", "唐", "陶", "腾", "田", "铁",
            "通", "佟", "涂", "吐", "万", "汪", "王", "韦", "卫", "魏",
            "温", "文", "翁", "乌", "吴", "伍", "武", "西", "奚", "席",
            "夏", "相", "向", "项", "萧", "肖", "谢", "辛", "邢", "熊",
            "修", "徐", "许", "续", "薛", "牙", "严", "颜", "杨", "姚",
            "叶", "衣", "依", "伊", "易", "益", "殷", "尹", "应", "雍",
            "尤", "游", "于", "余", "俞", "虞", "宇", "禹", "玉", "郁",
            "喻", "元", "袁", "岳", "云", "郓", "宰", "曾", "查", "翟",
            "詹", "张", "章", "赵", "折", "郑", "钟", "仲", "周", "朱",
            "诸", "祝", "庄", "卓", "宗", "邹", "祖", "左"
        };
        
        // 无效姓名模式
        private static readonly List<string> invalidPatterns = new List<string>(){
            "测试", "test", "123", "abc", "姓名", "名字", "用户", "玩家",
            "管理员", "admin", "root", "null", "undefined", "未知", "anonymous",
            "先生", "女士", "小姐", "同志", "老师", "教授", "老板", "经理"
        };
        
        /// <summary>
        /// 验证结果结构
        /// </summary>
        public struct ValidationResult
        {
            public bool IsValid;
            public string Message;
        
            public ValidationResult(bool isValid, string message)
            {
                IsValid = isValid;
                Message = message;
            }
        }

        /// <summary>
        /// 验证真实姓名有效性
        /// </summary>
        public static ValidationResult ValidateRealName(string name, bool fullCheck = false)
        {
            // 1. 基本清理和检查
            name = name.Trim();
            
            // 1.1 空值检查
            if (string.IsNullOrWhiteSpace(name))
                return new ValidationResult(false, "姓名不能为空");
            
            // 1.2 长度检查
            if (name.Length < minLength)
                return new ValidationResult(false, $"姓名太短，至少{minLength}个字");
            
            if (name.Length > maxLength)
                return new ValidationResult(false, $"姓名太长，最多{maxLength}个字");
            
            // 2. 字符集检查
            if (!IsValidNameCharacters(name))
                return new ValidationResult(false, "姓名包含非法字符");
            
            // 3. 格式检查
            if (!IsValidNameFormat(name))
                return new ValidationResult(false, "姓名格式不正确");
            
            // 如果是完整检查，进行更深入的验证
            if (fullCheck)
            {
                // 4. 常见姓氏检查
                if (checkCommonSurnames && !ContainsCommonSurname(name))
                    return new ValidationResult(false, "姓氏不常见，请确认是否正确");
                
                // 5. 复姓检查
                if (allowCompoundSurnames && IsPossibleCompoundSurname(name))
                {
                    if (!IsValidCompoundSurname(name))
                        return new ValidationResult(false, "复姓格式不正确");
                }
                
                // 6. 少数民族姓名检查
                if (checkEthnicNames && IsPossibleEthnicName(name))
                {
                    if (!IsValidEthnicName(name))
                        return new ValidationResult(false, "少数民族姓名格式不正确");
                }
                
                // 7. 无效模式检查
                if (checkInvalidPatterns && ContainsInvalidPattern(name))
                    return new ValidationResult(false, "姓名包含无效内容");
                
                // 8. 罕见姓名检查
                if (checkRareNames && IsRareNamePattern(name))
                    return new ValidationResult(false, "姓名过于罕见，请确认是否正确");
            }
            
            return new ValidationResult(true, "姓名有效");
        }
        
        /// <summary>
        /// 检查是否包含常见姓氏
        /// </summary>
        private static bool ContainsCommonSurname(string name)
        {
            // 检查单姓
            string firstChar = name.Substring(0, 1);
            if (commonSurnames.Contains(firstChar))
                return true;
            
            // 检查复姓
            if (name.Length >= 2 && allowCompoundSurnames)
            {
                string firstTwoChars = name.Substring(0, 2);
                if (compoundSurnames.Contains(firstTwoChars))
                    return true;
            }
            
            // 少数民族姓氏检查
            if (ethnicPrefixes.Contains(firstChar))
                return true;
            
            // 国际姓名不检查姓氏
            if (international) return true;
            
            return false;
        }
        
        /// <summary>
        /// 检查字符是否合法
        /// </summary>
        private static bool IsValidNameCharacters(string name)
        {
            // 中文姓名：只允许汉字和少数民族点号
            if (mainlandChina || taiwan || hongKongMacau)
            {
                string pattern = allowEthnicDots ? 
                    "^[\u4e00-\u9fa5·]+$" : 
                    "^[\u4e00-\u9fa5]+$";
                
                return Regex.IsMatch(name, pattern);
            }
            
            // 国际姓名：允许字母、空格、连字符、点号
            if (international)
            {
                return Regex.IsMatch(name, @"^[a-zA-Z\s\-\.']+$");
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查姓名格式
        /// </summary>
        private static bool IsValidNameFormat(string name)
        {
            // 中文姓名格式检查
            if (mainlandChina || taiwan || hongKongMacau)
            {
                // 不能包含连续重复字符
                if (Regex.IsMatch(name, @"(.)\1{2}")) // 连续3个相同字符
                    return false;
                
                // 不能包含数字
                if (Regex.IsMatch(name, @"\d"))
                    return false;
                
                // 点号使用规则（少数民族姓名）
                if (allowEthnicDots)
                {
                    // 点号不能在开头或结尾
                    if (name.StartsWith("·") || name.EndsWith("·"))
                        return false;
                    
                    // 点号不能连续出现
                    if (name.Contains("··"))
                        return false;
                }
            }
            
            // 国际姓名格式检查
            if (international)
            {
                // 开头和结尾不能是空格或符号
                if (name.StartsWith(" ") || name.StartsWith("-") || name.StartsWith(".") ||
                    name.EndsWith(" ") || name.EndsWith("-") || name.EndsWith("."))
                    return false;
                
                // 不能有连续空格
                if (name.Contains("  "))
                    return false;
                
                // 符号后必须有字母
                if (Regex.IsMatch(name, @"[\-\.'](\s|[\-\.']|$)"))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查是否为可能的复姓
        /// </summary>
        private static bool IsPossibleCompoundSurname(string name)
        {
            if (name.Length < 3) return false;
            return compoundSurnames.Contains(name.Substring(0, 2));
        }
        
        /// <summary>
        /// 验证复姓姓名格式
        /// </summary>
        private static bool IsValidCompoundSurname(string name)
        {
            // 复姓后至少有一个字
            if (name.Length < 3) return false;
            
            // 检查常见复姓格式
            string surname = name.Substring(0, 2);
            string givenName = name.Substring(2);
            
            // 复姓后不能直接跟点号（少数民族点号）
            if (givenName.StartsWith("·")) return false;
            
            return true;
        }
        
        /// <summary>
        /// 检查是否为可能的少数民族姓名
        /// </summary>
        private static bool IsPossibleEthnicName(string name)
        {
            if (name.Length < 2) return false;
            
            // 检查常见少数民族前缀
            string firstChar = name.Substring(0, 1);
            return ethnicPrefixes.Contains(firstChar);
        }
        
        /// <summary>
        /// 验证少数民族姓名格式
        /// </summary>
        private static bool IsValidEthnicName(string name)
        {
            // 少数民族姓名通常有分隔符
            if (!name.Contains("·")) return true; // 不是必须的
            
            // 检查点号位置
            int dotIndex = name.IndexOf('·');
            
            // 点号不能在开头或结尾
            if (dotIndex == 0 || dotIndex == name.Length - 1)
                return false;
            
            // 点号前后必须有文字
            if (dotIndex < 1 || dotIndex > name.Length - 2)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// 检查是否包含无效模式
        /// </summary>
        private static bool ContainsInvalidPattern(string name)
        {
            foreach (string pattern in invalidPatterns)
            {
                if (name.Contains(pattern))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// 检查是否为罕见姓名模式
        /// </summary>
        private static bool IsRareNamePattern(string name)
        {
            // 单字名在现代比较少见
            if (name.Length == 1) return true;
            
            // 非常长的姓名（超过6个字）
            if (name.Length > 6) return true;
            
            // 包含生僻字（这里简化为检查非常用字）
            // 实际项目中可以使用更完整的生僻字列表
            if (ContainsRareCharacters(name))
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 检查是否包含生僻字（简化版）
        /// </summary>
        private static bool ContainsRareCharacters(string name)
        {
            // 实际项目中应使用完整的生僻字列表
            // 这里只做演示
            List<string> rareChars = new List<string>() { "龘", "䶮", "䲜", "䨻", "𠔻", "𪚥" };
            foreach (char c in name)
            {
                if (rareChars.Contains(c.ToString()))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取姓氏部分
        /// </summary>
        public static string ExtractSurname(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            
            // 检查复姓
            if (name.Length >= 2 && allowCompoundSurnames)
            {
                string firstTwo = name.Substring(0, 2);
                if (compoundSurnames.Contains(firstTwo))
                    return firstTwo;
            }
            
            // 默认返回第一个字
            return name.Substring(0, 1);
        }
        
        /// <summary>
        /// 获取名字部分
        /// </summary>
        public static string ExtractGivenName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            
            // 复姓情况
            if (name.Length >= 2 && allowCompoundSurnames)
            {
                string firstTwo = name.Substring(0, 2);
                if (compoundSurnames.Contains(firstTwo))
                    return name.Substring(2);
            }
            
            // 少数民族姓名带点号的情况
            if (allowEthnicDots && name.Contains("·"))
            {
                int dotIndex = name.IndexOf('·');
                if (dotIndex > 0 && dotIndex < name.Length - 1)
                {
                    return name.Substring(dotIndex + 1);
                }
            }
            
            // 默认返回第一个字之后的部分
            return name.Length > 1 ? name.Substring(1) : "";
        }

        #endregion

        #region 验证身份证号码
        
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
        
        
        #endregion
        
    }
}