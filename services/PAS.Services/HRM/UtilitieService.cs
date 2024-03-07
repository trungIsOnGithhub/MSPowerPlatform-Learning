using AutoMapper;
using Microsoft.SharePoint.Client;
using OfficeOpenXml;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using PAS.Services.HRM.TOTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PAS.Services.HRM
{

    public interface IUtilitieService : IBaseService
    {
        bool checkOTP(int userId, string strOPT);
        string GetSecrectKey(int userId);
        bool AddQRCode(int userId, string loginName);
        bool ResetQRCode(int userId);
    }

    public class UtilitieService : BaseService, IUtilitieService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UtilitieService(IHRMUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
            : base(unitOfWork)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public string GetSecrectKey(int userId)
        {

            string secrectKey = "";
            var otp = _unitOfWork.OtpRepository.Find(t => t.UserId == userId).FirstOrDefault();
            if (otp != null)
            {
                if (otp.IsShowQRCode)
                {
                    secrectKey = otp.SecretKey;
                }
            }
            return secrectKey;

        }

        public bool ResetQRCode(int userId)
        {

            var user = _unitOfWork.UserRepository.Find(t => t.Id == userId).FirstOrDefault();

            if (user != null)
            {
                var userOpt = _unitOfWork.OtpRepository.Find(t => t.UserId == user.Id).FirstOrDefault();
                if (userOpt != null)
                {
                    userOpt.SecretKey = GenerateSerectKey();
                    userOpt.IsShowQRCode = true;

                    _unitOfWork.Save();
                }
                else
                {
                    var userOtp = new PAS.Repositories.DataModel.Otp
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        SecretKey = GenerateSerectKey(),
                        IsShowQRCode = true,
                        LoginName = user.LoginName
                    };
                    _unitOfWork.OtpRepository.Add(userOtp);
                    _unitOfWork.Save();

                }
            }
            return true;

        }

        public bool AddQRCode(int userId, string loginName)
        {

            var userOpt = _unitOfWork.OtpRepository.Find(t => t.LoginName == loginName).FirstOrDefault();

            if (userOpt == null)
            {
                var newOtp = new PAS.Repositories.DataModel.Otp
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SecretKey = GenerateSerectKey(),
                    IsShowQRCode = true,
                    LoginName = loginName
                };

                var newOtpEntity = _mapper.Map<PAS.Repositories.DataModel.Otp>(newOtp);
                _unitOfWork.OtpRepository.Add(newOtpEntity);
                _unitOfWork.Save();
            }
            return true;

        }

        public bool checkOTP(int userId, string strOPT)
        {
            bool result = false;
            var opt = _unitOfWork.OtpRepository.Find(t => t.UserId == userId).FirstOrDefault();
            result = (strOPT == new TOTP.TOTP().GeneratePin(opt.SecretKey));
            if (opt.IsShowQRCode && result)
            {
                opt.IsShowQRCode = false;
                _unitOfWork.Save();
            }
            return result;
        }

      
        private void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private string GenerateSerectKey()
        {
            RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[10];
            rnd.GetBytes(randomBytes);
            return Transcoder.Base32Encode(randomBytes);
        }
    }
}

