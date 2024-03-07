using AutoMapper;
using Microsoft.SharePoint.Client;
using PAS.Model.Enum.HRM;
using PAS.Model.HRM.ClaimModels;
using PAS.Repositories;
using PAS.Repositories.HRM;
using PAS.Services.HRM.Infrastructures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PAS.Services.HRM
{

    public interface IClaimService : IBaseService
    {
        ClaimRequest UpdateClaimRequest(ClaimRequest claimRequest, ClientContext context, Guid relatedUserId);
        System.Threading.Tasks.Task CreateClaimRequest(ClaimRequestPost claimRequest, Guid tenantId);
        void UpdateClaimRequest(UpdateClaimRequestPost claimRequest, Guid relatedUserId);
        ClaimRequest GetClaimRequestById(ClientContext ctx, Guid claimRequestId);
        ClaimRequest GetClaimRequestById(Guid claimRequestId);
        ClaimPaging GetListOfClaimRequestByUserId(Guid userId, int page, int itemsPerPage, ClaimFilter filter);
        ClaimPaging GetListOfClaimRequest(int page, int itemsPerPage, ClaimFilter filter);
        List<ClaimRequest> GetListOfClaimRequestByUserId(Guid userId, int? c, ClaimApprovalStep? s, DateTime? e, int currentRequestCount);
        List<ClaimRequest> GetListOfClaimRequestByUserId(Guid userId, int? c, ClaimApprovalStep? s, DateTime? e);
        void RemoveClaimRequestById(Guid claimRequestId, bool isRequester);
        void RemoveClaimRequestById(Guid claimRequestId, string comment, bool isRequester);
        List<ClaimRequestHistory> GetClaimRequestHistoryByClaimId(Guid claimRequestId);
        ClaimRequest ApproveClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId);
        ClaimRequest RejectClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId);
        ClaimRequest PayClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId);
        ClaimRequest RemoveClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId);
        ClaimSummary GetMyClaimSummary(Guid userId, int currencyId);
    }
    public class ClaimService : BaseService, IClaimService
    {
        private IUtilitieService _utilitieService;
        private IMapper _mapper;
        public ClaimService(IHRMUnitOfWork unitOfWork, CoreContext context, IUtilitieService utilitieService, IMapper mapper) : base(unitOfWork)
        {
            _utilitieService = utilitieService;
            _mapper = mapper;
        }

        #region Create claim request
        public async System.Threading.Tasks.Task CreateClaimRequest(ClaimRequestPost claimRequestPost, Guid tenantId)
        {
            //create claim request//==================================================================
            //map to entity

            var claimRequestEntity = MapFromClaimRequestPostToClaimRequestEntity(claimRequestPost);
            claimRequestEntity.ClaimRequestHistories = new List<PAS.Repositories.DataModel.ClaimRequestHistory>
                {
                    new PAS.Repositories.DataModel.ClaimRequestHistory
                    {
                        Id = Guid.NewGuid(),
                        Action = ClaimUserAction.Create,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        UserId = claimRequestPost.RequesterId,
                        User = null
                    }
                };
            if (claimRequestPost.ClaimRequestInvoices != null)
            {
                claimRequestEntity.ClaimRequestInvoices = new List<PAS.Repositories.DataModel.ClaimRequestInvoice>();
                foreach (var invoice in claimRequestPost.ClaimRequestInvoices)
                {   //when attachment null create claim request invoice without attachment
                    if (invoice.Attachment != null)
                    {
                        var claimRequestInvoice = new PAS.Repositories.DataModel.ClaimRequestInvoice()
                        {
                            Status = ClaimInvoiceStatus.Pending,
                            Attachment = invoice.Attachment.FolderUrl + "/" + invoice.Attachment.FileName,
                            ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId,
                            Total = invoice.Total
                        };

                        claimRequestEntity.ClaimRequestInvoices.Add(claimRequestInvoice);
                    }
                    else
                    {
                        var claimRequestInvoice = new PAS.Repositories.DataModel.ClaimRequestInvoice()
                        {
                            Status = ClaimInvoiceStatus.Pending,
                            ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId,
                            Total = invoice.Total
                        };

                        claimRequestEntity.ClaimRequestInvoices.Add(claimRequestInvoice);
                    }
                }
            }
            _unitOfWork.ClaimRequestRepository.Add(claimRequestEntity);
            await _unitOfWork.SaveAsync();
        }

        #endregion

        #region Update claim request
        public void UpdateClaimRequest(UpdateClaimRequestPost updateClaimRequestPost, Guid relatedUserId)
        {
            //update claim request//==================================================================
            //map to entity

            var oldClaimEntity = _unitOfWork.ClaimRequestRepository.FindById(updateClaimRequestPost.Id);

            if (updateClaimRequestPost.ClaimRequestInvoices != null)
            {
                oldClaimEntity.ExpenseDate = updateClaimRequestPost.ExpenseDate;
                oldClaimEntity.Total = updateClaimRequestPost.Total;
                oldClaimEntity.ClaimTypeId = updateClaimRequestPost.ClaimTypeId;
                oldClaimEntity.CurrencyId = updateClaimRequestPost.CurrencyId;

                var claimRequestHistory = new ClaimRequestHistory()
                {
                    UserId = relatedUserId,
                    ClaimRequestId = updateClaimRequestPost.Id,
                };
                AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Update);

                Parallel.ForEach(updateClaimRequestPost.RemoveInvoices, item =>
                {
                    var deleteInvoice = _unitOfWork.ClaimRequestInvoiceRepository.FindById(item);
                    if (deleteInvoice != null)
                    {
                        _unitOfWork.ClaimRequestInvoiceRepository.Delete(deleteInvoice);
                    }
                });

                foreach (var invoice in updateClaimRequestPost.ClaimRequestInvoices)
                {

                    if (invoice.Id <= -1)
                    {
                        if (invoice.Attachment != null)
                        {
                            var claimRequestInvoice = new PAS.Repositories.DataModel.ClaimRequestInvoice()
                            {
                                Status = ClaimInvoiceStatus.Pending,
                                Attachment = invoice.Attachment.FolderUrl + "/" + invoice.Attachment.FileName,
                                ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId,
                                Total = invoice.Total,
                                ClaimRequestId = updateClaimRequestPost.Id
                            };

                            oldClaimEntity.ClaimRequestInvoices.Add(claimRequestInvoice);
                        }
                        else
                        {
                            var claimRequestInvoice = new PAS.Repositories.DataModel.ClaimRequestInvoice()
                            {
                                Status = ClaimInvoiceStatus.Pending,
                                ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId,
                                Total = invoice.Total,
                                ClaimRequestId = updateClaimRequestPost.Id
                            };

                            oldClaimEntity.ClaimRequestInvoices.Add(claimRequestInvoice);
                        }
                    }
                }
            }
            _unitOfWork.Save();
        }

        #endregion

        #region Get Claim Detail
        public ClaimRequest GetClaimRequestById(ClientContext ctx, Guid claimRequestId)
        {
            var claimRequest = _unitOfWork.ClaimRequestRepository
                            .Find(x => x.Id == claimRequestId, includeProperties:
                                "Requester.UserInformation," +
                                "ClaimRequestInvoices.ClaimInvoiceCategory.ClaimInvoiceCategoryLocalizations," +
                                "ClaimType," +
                                "ClaimType.ClaimTypeLocalizations," +
                                "Currency," +
                                "ActualCurrency," +
                                "ClaimRequestHistories.User.UserInformation").FirstOrDefault();

            var result = _mapper.Map<ClaimRequest>(claimRequest);
            if (result.ClaimTypeId != 0)
            {
                result.ClaimType.Id = result.ClaimTypeId;
            }
            foreach (var invoice in result.ClaimRequestInvoices)
            {
                var url = invoice.Attachment;
                var file = ctx.Web.GetFileByUrl(url);
                var stream = file.OpenBinaryStream();
                ctx.Load(file);
                ctx.ExecuteQuery();
                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                {


                    stream.Value.CopyTo(mStream);

                    var base64 = Convert.ToBase64String(mStream.ToArray());
                    invoice.Attachment = base64;
                    invoice.FileName = Path.GetFileName(url);


                }


            }
            return result;
        }
        public List<ClaimRequestHistory> GetClaimRequestHistoryByClaimId(Guid claimRequestId)
        {
            var result = _unitOfWork.ClaimRequestHistoryRepository
                                    .Find(t => t.ClaimRequestId == claimRequestId, includeProperties: "User.UserInformation")
                                    .AsEnumerable()
                                    .Select(t => _mapper.Map<ClaimRequestHistory>(t))
                                    .OrderByDescending(x => x.Created)
                                    .ToList();
            return result;
        }
        #endregion

        #region Get List Of Claim Request

        //For PWA call
        public List<ClaimRequest> GetListOfClaimRequestByUserId(Guid userId, int? claimTypeId, ClaimApprovalStep? s, DateTime? e, int currentRequestCount)
        {
            var claimRequestQuery = GetListOfClaimRequestByUserIdContent(userId).Where(t => !t.IsRemoved);
            if (claimTypeId != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.ClaimTypeId == claimTypeId);
            }
            if (s != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.Status == s);
            }

            if (e != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.ExpenseDate.Date == e.Value.Date);
            }

            claimRequestQuery = claimRequestQuery.Skip(currentRequestCount).Take(10);

            return claimRequestQuery.Select(request => _mapper.Map<ClaimRequest>(request)).ToList();
        }

        public List<ClaimRequest> GetListOfClaimRequestByUserId(Guid userId, int? claimTypeId, ClaimApprovalStep? s, DateTime? e)
        {
            var claimRequestQuery = GetListOfClaimRequestByUserIdContent(userId);

            if (claimTypeId != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.ClaimTypeId == claimTypeId);
            }
            if (s != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.Status == s);
            }

            if (e != null)
            {
                claimRequestQuery = claimRequestQuery.Where(x => x.ExpenseDate.Date == e.Value.Date);
            }
            return claimRequestQuery.Select(request => _mapper.Map<ClaimRequest>(request)).ToList();
        }

        public ClaimPaging GetListOfClaimRequestByUserId(Guid userId, int page, int itemsPerPage, ClaimFilter filter)
        {
            var currentYear = DateTime.Now.Year;
            var claimRequestQuery = GetListOfClaimRequestByUserIdContent(userId).Where(x => x.ExpenseDate.Year == currentYear);
            claimRequestQuery = FilterClaimRequest(claimRequestQuery, filter);
            var result = Pagination(claimRequestQuery, page, itemsPerPage);
            return result;
        }

        public ClaimSummary GetMyClaimSummary(Guid userId, int currencyId)
        {
            var claimPaging = new ClaimPaging();
            var claimRequestQuery = GetListOfClaimRequestByUserIdContent(userId);
            var claimSummary = new ClaimSummary();
            var claimRequest = claimRequestQuery.Where(x => !x.IsRemoved).ToList();
            claimSummary.TotalRequest = claimRequest.Count();

            var rejectClaims = claimRequestQuery.Where(x => x.Status == ClaimApprovalStep.Rejected && x.CurrencyId == currencyId && !x.IsRemoved).ToList();
            claimSummary.TotalRejected = rejectClaims.Sum(x => x.Total);

            var pendingClaims = claimRequestQuery.Where(x => (x.Status == ClaimApprovalStep.Pending || x.Status == ClaimApprovalStep.Approved) && x.CurrencyId == currencyId && !x.IsRemoved).ToList();
            claimSummary.TotalWaiting = pendingClaims.Sum(x => x.Total);

            var actualPaidClaims = claimRequestQuery.Where(x => x.Status == ClaimApprovalStep.Paid && x.ActualCurrencyId == currencyId && !x.IsRemoved).ToList();
            claimSummary.ActualTotalPaid = actualPaidClaims.Sum(x => x.ActualTotal);

            return claimSummary;
        }

        private float? ConvertCurrencyFromExchangeRate(IList<PAS.Repositories.DataModel.ClaimRequest> claimRequests, int currencyId)
        {
            var total = 0F;
            foreach (var claimRequest in claimRequests)
            {
                var amount = 0F;
                if (claimRequest.CurrencyId != currencyId)
                {
                    var exchangeRateFrom = _unitOfWork.CurrencyDetailRepository.Find(x => x.FromCurrencyId == claimRequest.CurrencyId && x.ToCurrencyId == currencyId).FirstOrDefault();
                    if (exchangeRateFrom != null)
                    {
                        amount = claimRequest.Total * exchangeRateFrom.ExchangeRateFrom;
                    }
                    else
                    {
                        var exchangeRateTo = _unitOfWork.CurrencyDetailRepository.Find(x => x.FromCurrencyId == currencyId && x.ToCurrencyId == claimRequest.CurrencyId).FirstOrDefault();
                        if (exchangeRateTo != null)
                        {
                            amount = claimRequest.Total * exchangeRateTo.ExchangeRateTo;
                        }
                    }
                }
                else
                {
                    amount = claimRequest.Total;
                }
                total += amount;
            }
            return total;
        }

        public ClaimPaging GetListOfClaimRequest(int page, int itemsPerPage, ClaimFilter filter)
        {
            var claimPaging = new ClaimPaging();
            var claimRequestQuery = _unitOfWork.ClaimRequestRepository.Find(x => x.Status != ClaimApprovalStep.Removed)
                                   .Include(t => t.ClaimRequestInvoices)
                                   .Include(t => t.Currency)
                                   .Include(t => t.ClaimType.ClaimTypeLocalizations)
                                   .OrderByDescending(x => x.ExpenseDate)
                                   .AsEnumerable();
            claimRequestQuery = FilterClaimRequest(claimRequestQuery, filter);
            var result = Pagination(claimRequestQuery, page, itemsPerPage);
            return result;
        }
        #endregion

        #region Remove Claim Request
        public void RemoveClaimRequestById(Guid claimRequestId, bool isRequester)
        {
            var claimRequestEnitity = _unitOfWork.ClaimRequestRepository.Find(x => x.Id == claimRequestId).FirstOrDefault();
            claimRequestEnitity.Status = ClaimApprovalStep.Removed;
            claimRequestEnitity.ClaimRequestHistories.Add(
                new PAS.Repositories.DataModel.ClaimRequestHistory
                {
                    Id = Guid.NewGuid(),
                    Action = ClaimUserAction.Remove,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    UserId = claimRequestEnitity.RequesterId
                });
            _unitOfWork.Save();
        }
        #endregion

        public void RemoveClaimRequestById(Guid claimRequestId, string comment, bool isRequester)
        {
            var claimRequestEnitity = _unitOfWork.ClaimRequestRepository.Find(x => x.Id == claimRequestId).FirstOrDefault();
            claimRequestEnitity.Status = ClaimApprovalStep.Removed;
            claimRequestEnitity.ClaimRequestHistories.Add(
                new PAS.Repositories.DataModel.ClaimRequestHistory
                {
                    Id = Guid.NewGuid(),
                    Action = ClaimUserAction.Remove,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    UserId = claimRequestEnitity.RequesterId,
                    Comment = comment
                });
            _unitOfWork.Save();
        }

        public ClaimRequest ApproveClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId)
        {
            var claimRequestEntity = _unitOfWork.ClaimRequestRepository
                                                .FindById(claimRequestHistoryPost.ClaimRequestId);

            if (claimRequestEntity.Status == ClaimApprovalStep.Pending)
            {
                // history
                var claimRequestHistory = _mapper.Map<ClaimRequestHistory>(claimRequestHistoryPost);
                AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Approve);

                // update claim Request status
                claimRequestEntity.Status = ClaimApprovalStep.Approved;
                claimRequestEntity.Modified = DateTime.UtcNow;

                _unitOfWork.Save();
            }

            var claimRequest = _mapper.Map<ClaimRequest>(claimRequestEntity);
            return claimRequest;
        }

        public ClaimRequest RejectClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId)
        {
            var claimRequestEntity = _unitOfWork.ClaimRequestRepository
                                                .FindById(claimRequestHistoryPost.ClaimRequestId);

            // Approver reject a request
            if (claimRequestEntity.Status == ClaimApprovalStep.Pending || claimRequestEntity.Status == ClaimApprovalStep.Approved)
            {
                // history
                var claimRequestHistory = _mapper.Map<ClaimRequestHistory>(claimRequestHistoryPost);
                AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Reject);

                // update claim Request status
                claimRequestEntity.Status = ClaimApprovalStep.Rejected;
                claimRequestEntity.Modified = DateTime.UtcNow;

                _unitOfWork.Save();
            }

            var claimRequest = _mapper.Map<ClaimRequest>(claimRequestEntity);
            return claimRequest;
        }

        public ClaimRequest PayClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId)
        {
            var claimRequestEntity = _unitOfWork.ClaimRequestRepository
                                                .FindById(claimRequestHistoryPost.ClaimRequestId);

            if (claimRequestEntity.Status == ClaimApprovalStep.Approved)
            {
                // history
                var claimRequestHistory = _mapper.Map<ClaimRequestHistory>(claimRequestHistoryPost);
                AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Pay);

                // update claim Request status
                claimRequestEntity.ActualCurrencyId = claimRequestHistoryPost.ActualCurrencyId;
                claimRequestEntity.ActualExchangeRate = claimRequestHistoryPost.ActualExchangeRate;
                claimRequestEntity.ActualTotal = claimRequestHistoryPost.ActualTotal;
                claimRequestEntity.Status = ClaimApprovalStep.Paid;
                claimRequestEntity.Modified = DateTime.UtcNow;

                _unitOfWork.Save();
            }

            var claimRequest = _mapper.Map<ClaimRequest>(claimRequestEntity);
            return claimRequest;
        }

        public ClaimRequest RemoveClaimRequest(ClaimRequestHistoryPost claimRequestHistoryPost, Guid tenantId)
        {
            // history
            var claimRequestHistory = _mapper.Map<ClaimRequestHistory>(claimRequestHistoryPost);
            AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Remove);
            // update Claim Request Status
            var claimRequestEntity = _unitOfWork.ClaimRequestRepository
                .FindById(claimRequestHistoryPost.ClaimRequestId);
            claimRequestEntity.IsRemoved = true;
            claimRequestEntity.Modified = DateTime.UtcNow;

            _unitOfWork.Save();

            var claimRequest = _mapper.Map<ClaimRequest>(claimRequestEntity);
            return claimRequest;
        }

        #region Private
        private IEnumerable<PAS.Repositories.DataModel.ClaimRequest> GetListOfClaimRequestByUserIdContent(Guid userId)
        {
            var claimRequestQuery = _unitOfWork.ClaimRequestRepository.Find(x => x.RequesterId == userId && x.Status != ClaimApprovalStep.Removed)
                                               .Include(t => t.ClaimRequestInvoices)
                                               .Include(t => t.Currency)
                                               .Include(t => t.ClaimType.ClaimTypeLocalizations)
                                               .OrderByDescending(x => x.ExpenseDate);
            return claimRequestQuery;
        }

        private IEnumerable<PAS.Repositories.DataModel.ClaimRequest> FilterClaimRequest(IEnumerable<PAS.Repositories.DataModel.ClaimRequest> claimRequests, ClaimFilter filter)
        {
            if (filter.ClaimType != null && filter.ClaimType.Code != null)
            {
                claimRequests = claimRequests.Where(x => x.ClaimType.Code == filter.ClaimType.Code);
            }

            if (filter.Status != null)
            {
                if (filter.Status.Value == (int)ClaimApprovalStep.Removed)
                {
                    claimRequests = claimRequests.Where(x => x.IsRemoved);
                }
                else
                {
                    claimRequests = claimRequests.Where(x => x.Status == (ClaimApprovalStep)filter.Status.Value && !x.IsRemoved);
                }
            }

            if (filter.ExpenseDate != DateTime.MinValue)
            {
                claimRequests = claimRequests.Where(x => x.ExpenseDate.Date >= filter.ExpenseDate.Date);
            }
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                claimRequests = claimRequests.Where(x => x.Requester.Name.ToLower().Contains(filter.SearchString.ToLower()));
            }

            return claimRequests;
        }

        private ClaimPaging Pagination(IEnumerable<PAS.Repositories.DataModel.ClaimRequest> searchClaimList, int page, int itemsPerPage)
        {
            var claimPaging = new ClaimPaging();
            var totalCount = searchClaimList.Count();
            claimPaging.TotalItems = totalCount;
            claimPaging.TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);
            if (page <= claimPaging.TotalPages && page >= 1)
            {
                if (totalCount > itemsPerPage)
                {
                    var claims = searchClaimList.OrderByDescending(t => t.Modified).Skip(itemsPerPage * (page - 1))
                        .Take(itemsPerPage)
                        .OrderByDescending(t => t.ExpenseDate)
                        .ToList();
                    claimPaging.ClaimRequests = _mapper.Map<List<ClaimRequest>>(claims);
                }
                else
                {
                    var claims = searchClaimList
                        .OrderByDescending(t => t.ExpenseDate)
                        .ToList();
                    claimPaging.ClaimRequests = _mapper.Map<List<ClaimRequest>>(claims);
                }
            }
            return claimPaging;
        }

        private ClaimRequestHistory AddClaimRequestHistory(ClaimRequestHistory claimRequestHistory, ClaimUserAction claimUserAction)
        {
            claimRequestHistory.Id = Guid.NewGuid();
            claimRequestHistory.Action = claimUserAction;
            claimRequestHistory.Created = DateTime.UtcNow;
            claimRequestHistory.Modified = DateTime.UtcNow;

            var claimRequestHistoryEntity = _mapper.Map<PAS.Repositories.DataModel.ClaimRequestHistory>(claimRequestHistory);
            claimRequestHistoryEntity.User = null;
            _unitOfWork.ClaimRequestHistoryRepository.Add(claimRequestHistoryEntity);
            _unitOfWork.Save();

            return claimRequestHistory;
        }

        private PAS.Repositories.DataModel.ClaimRequest MapFromClaimRequestPostToClaimRequestEntity(ClaimRequestPost claimRequestPost)
        {
            var newClaimRequest = _mapper.Map<ClaimRequest>(claimRequestPost);

            newClaimRequest.Id = Guid.NewGuid();
            newClaimRequest.Status = ClaimApprovalStep.Pending;
            newClaimRequest.Stage = ClaimRequestStage.Publish;

            newClaimRequest.ActualCurrencyId = newClaimRequest.CurrencyId;

            newClaimRequest.Created = DateTime.UtcNow;
            newClaimRequest.CreatedBy = claimRequestPost.RequesterId;
            newClaimRequest.Modified = DateTime.UtcNow;
            newClaimRequest.ModifiedBy = claimRequestPost.RequesterId;

            var claimRequestEntity = _mapper.Map<PAS.Repositories.DataModel.ClaimRequest>(newClaimRequest);
            claimRequestEntity.Requester = null;
            claimRequestEntity.ClaimType = null;
            return claimRequestEntity;
        }

        public ClaimRequest GetClaimRequestById(Guid claimRequestId)
        {
            var claimRequest = _unitOfWork.ClaimRequestRepository
                              .Find(x => x.Id == claimRequestId, includeProperties:
                                  "Requester.UserInformation," +
                                  "ClaimRequestInvoices.ClaimInvoiceCategory.ClaimInvoiceCategoryLocalizations," +
                                  "ClaimType.ClaimTypeLocalizations," +
                                  "Currency," +
                                  "ActualCurrency," +
                                  "ClaimRequestHistories.User.UserInformation").FirstOrDefault();
            var result = _mapper.Map<ClaimRequest>(claimRequest);
            return result;
        }

        public ClaimRequest UpdateClaimRequest(ClaimRequest claimRequest, ClientContext context, Guid relatedUserId)
        {

            var claimEntity = _unitOfWork.ClaimRequestRepository.FindById(claimRequest.Id);
            if (claimEntity.Status == ClaimApprovalStep.Pending)
            {
                claimEntity.Total = claimRequest.Total;
                claimEntity.ClaimTypeId = claimRequest.ClaimType.Id;
                claimEntity.ExpenseDate = claimRequest.ExpenseDate;
                claimEntity.CurrencyId = claimRequest.Currency.Id;
                _unitOfWork.ClaimRequestRepository.Update(claimEntity);

                var claimRequestHistory = new ClaimRequestHistory()
                {
                    UserId = relatedUserId,
                    ClaimRequestId = claimRequest.Id,
                };
                AddClaimRequestHistory(claimRequestHistory, ClaimUserAction.Update);

                var invoiceIds = claimRequest.ClaimRequestInvoices.Select(x => x.Id);
                ///remove old Invoice
                var removeInvoices = _unitOfWork.ClaimRequestInvoiceRepository.Find(x => x.ClaimRequestId == claimRequest.Id && !invoiceIds.Contains(x.Id));
                foreach (var invoice in removeInvoices)
                {
                    _unitOfWork.ClaimRequestInvoiceRepository.Delete(invoice);
                }
                /// update Invoice
                var updateInvoices = claimRequest.ClaimRequestInvoices.Where(x => x.Id != 0);
                foreach (var invoice in updateInvoices)
                {
                    var invoiceEntity = _unitOfWork.ClaimRequestInvoiceRepository.FindById(invoice.Id);
                    invoiceEntity.Total = invoice.Total;
                    invoice.ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId;
                    _unitOfWork.ClaimRequestInvoiceRepository.Update(invoiceEntity);
                }
                /// add new Invoice
                var addInvoices = claimRequest.ClaimRequestInvoices.Where(x => x.Id == 0);
                foreach (var invoice in addInvoices)
                {
                    var file = invoice.AttachmentFile;
                    var invoiceEntity = new PAS.Repositories.DataModel.ClaimRequestInvoice();
                    invoiceEntity.ClaimRequestId = claimRequest.Id;
                    invoiceEntity.Total = invoice.Total;
                    invoiceEntity.ClaimInvoiceCategoryId = invoice.ClaimInvoiceCategoryId;
                    invoiceEntity.Status = ClaimInvoiceStatus.Pending;
                    invoiceEntity.Attachment = file.FolderUrl + "/" + file.FileName;
                    _unitOfWork.ClaimRequestInvoiceRepository.Add(invoiceEntity);
                }

            }

            _unitOfWork.Save();
            return claimRequest;


            //var attachmentList = claimRequest.ClaimRequestInvoices.Select(t => t.Attachment);
            //foreach (var file in attachmentList)
            //{
            //    using (var clientContext = this.CreateContextFor(this.SPUrl, true))
            //    {
            //        _utilitieService.UploadFile(clientContext, file.FolderUrl, file.FileName, file.FileByte);
            //    }
            //}
        }
        #endregion
    }
}
