using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Ntl.AuthSS.Notification.Entities.EmailModel;
using System.Linq;
using System.Threading.Tasks;
using Ntl.AuthSS.OrderManagement.Business.Services;

namespace Ntl.AuthSS.Order_Management.Api.Models
{
    public class EmailNotificationTemplate
    {
        private IConfiguration _configuration;
        private UserClient _userClient;
        public EmailNotificationTemplate(IConfiguration configuration, UserClient userClient)
        {
            _configuration = configuration;
            _userClient = userClient;
        }
        public List<NotificationModel> BuildOrderPlacedTemplateForOrgUsers(Guid orderId, string orderNumber, int orgId, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderSubmitted = new OrderSubmitted();
                orderSubmitted.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderSubmitted.Subject = "Order Placed Successfully";
                orderSubmitted.OrderNumber = orderNumber;
                orderSubmitted.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderSubmitted.UserName = user.userName;
                orderSubmitted.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderSubmitted;
                notificationModel.EmailContentModelType = "OrderSubmittedForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderPlacedTemplateForPortalAdmins(Guid orderId, string orderNumber, string orgName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string>() { "TaxAuthAdmin", "TaxAuthRevenueOfficer" });
            foreach (var user in users)
            {
                var orderSubmitted = new OrderSubmitted
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Order placed by " + orgName,
                    MfName = orgName,
                    UserName = user.userName,
                    OrderNumber = orderNumber,
                    To = new List<string> { user.email },
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderReview/" + orderId
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderSubmitted;
                notificationModel.EmailContentModelType = "OrderSubmittedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderRejectedTemplateForOrgUsers(Guid orderId, int orgId, string orderNumber, string RejectComment, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderRejected = new OrderRejected();
                orderRejected.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderRejected.Subject = "Order with number " + orderNumber + " is rejected by Tax Authority";
                orderRejected.UserName = user.userName;
                orderRejected.OrderNumber = orderNumber;
                orderRejected.RejectedComment = RejectComment;
                orderRejected.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderRejected.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderRejected;
                notificationModel.EmailContentModelType = "OrderRejectedForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderRejectedTemplateForPortalAdmins(Guid orderId, string orderNumber, string RejectComment)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string>() { "TaxAuthAdmin", "TaxAuthRevenueOfficer" });
            foreach (var user in users)
            {
                var orderRejected = new OrderRejected();
                orderRejected.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderRejected.Subject = "Order with number " + orderNumber + " is rejected by Tax Authority";
                orderRejected.UserName = user.userName;
                orderRejected.OrderNumber = orderNumber;
                orderRejected.RejectedComment = RejectComment;
                orderRejected.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderRejected.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderRejected;
                notificationModel.EmailContentModelType = "OrderRejectedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public NotificationModel BuildOrderRejectedTemplateForSubmittedUser(Guid orderId, string orderNumber, string RejectComment, int userId)
        {
            var user = GetUserEmailIdByUserId(userId);
            var orderRejected = new OrderRejected();
            orderRejected.From = _configuration.GetValue<string>("NotificationFromEmail");
            orderRejected.Subject = "Order with number " + orderNumber + " is rejected by Tax Authority";
            orderRejected.UserName = user.UserName;
            orderRejected.OrderNumber = orderNumber;
            orderRejected.RejectedComment = RejectComment;
            orderRejected.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
            orderRejected.To = new List<string> { user.EmailId };
            NotificationModel notificationModel = new NotificationModel();
            notificationModel.EmailContent = orderRejected;
            notificationModel.EmailContentModelType = "OrderRejectedForSubmittedUser";
            notificationModel.NotificationType = NotificationType.Email;
            return notificationModel;
        }
        public List<NotificationModel> BuildOrderApprovedTemplateForOrgUsers(Guid orderId, int orgId, string orderNumber, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderApproved = new OrderApproved();
                orderApproved.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderApproved.Subject = "Order with number " + orderNumber + " is Approved by Tax Authority";
                orderApproved.OrderNumber = orderNumber;
                orderApproved.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderApproved.UserName = user.userName;
                orderApproved.To = new List<string> { user.email };
                orderApproved.AttachementFileName = orderNumber.ToLower();
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderApproved;
                notificationModel.EmailContentModelType = "OrderApprovedForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderApprovedTemplateForPortalAdmins(Guid orderId, string orderNmbr, string orgName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string> { "TsspAdmin", "TsspIntermediate", "TsspWarehouseIncharge" });

            foreach (var user in users)
            {
                var orderApproved = new OrderApproved
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Order with number " + orderNmbr + " of " + orgName + " is approved",
                    MfName = orgName,
                    UserName = user.userName,
                    OrderNumber = orderNmbr,
                    AttachementFileName = orderNmbr.ToLower(),
                    To = new List<string> { user.email },
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/Fullfill/" + orderId
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderApproved;
                notificationModel.EmailContentModelType = "OrderApprovedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }

        //
        public List<NotificationModel> BuildOrderApprovedTemplateForFinanceOfficer(Guid orderId, string orderNmbr, string orgName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            //var users = GetUsersEmailIdByRole(new List<string> { "TsspAdmin", "TsspIntermediate", "TsspWarehouseIncharge" });
            var users = new List<(string userName, string email)>();
            var financiarEmail = _configuration.GetValue<string>("FinanciarEmail");
            var emails = financiarEmail.Split(',').ToList();
            foreach (var email in emails)
            {
                //var data = new List<(string userName, string email)>();
                users.Add(("Finance Officer", email.Trim()));
            }

            foreach (var user in users)
            {
                var orderApproved = new OrderApproved
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    //Subject = "Order with number " + orderNmbr + " of " + orgName + " is approved",
                    Subject = "Order Invoice Copy",
                    MfName = orgName,
                    UserName = user.userName,
                    OrderNumber = orderNmbr,
                    AttachementFileName = orderNmbr.ToLower(),
                    To = new List<string> { user.email },
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/Fullfill/" + orderId
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderApproved;
                notificationModel.EmailContentModelType = "OrderApprovedForFinanceOfficer";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildChangeSkuRequestedTemplateForOrgUsers(string productname, string sku, string mfName, int orgId, string warehouseEmail = "")
        {
            var lstNotificationModel = new List<NotificationModel>();
            string comment; string subject;
            if (productname != null && sku != null)
            {
                comment = mfName + " has submitted a request for change of " + sku + " and change of " + productname + "";
                subject = mfName + " submitted a request for change of " + sku + " and " + productname + "";
            }
            else if (productname == null && sku != null)
            {
                comment = mfName + " has submitted a request for change of " + sku + "";
                subject = mfName + " submitted a request for change of " + sku + "";
            }
            else
            {
                comment = mfName + " has submitted a request for change of change of " + productname + "";
                subject = mfName + " submitted a request for change of " + productname + "";
            }
            var users = GetUsersEmailIdByOrgId(orgId);
            //var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            //neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var changeSkuRequested = new ChangeSkuRequested
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = subject,
                    MfName = mfName,
                    ProductName = productname,
                    MainMessage = comment,
                    To = new List<string> { user.email },
                    UserName = user.userName,
                    SKU = sku,
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = changeSkuRequested;
                notificationModel.EmailContentModelType = "ChangeSkuRequestedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildChangeSkuRequestedTemplateForPortalAdmins(string productname, string sku, string mfName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            string comment; string subject;
            if (productname != null && sku != null)
            {
                comment = mfName + " has submitted a request for change of " + sku + " and change of " + productname + "";
                subject = mfName + " submitted a request for change of " + sku + " and " + productname + "";
            }
            else if (productname == null && sku != null)
            {
                comment = mfName + " has submitted a request for change of " + sku + "";
                subject = mfName + " submitted a request for change of " + sku + "";
            }
            else
            {
                comment = mfName + " has submitted a request for change of change of " + productname + "";
                subject = mfName + " submitted a request for change of " + productname + "";
            }
            var users = GetUsersEmailIdByRole(new List<string>() { "TaxAuthAdmin", "TaxAuthRevenueOfficer", "TsspAdmin", "TsspIntermediate", "TsspWarehouseIncharge" });
            foreach (var user in users)
            {
                var changeSkuRequested = new ChangeSkuRequested
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = subject,
                    MfName = mfName,
                    ProductName = productname,
                    MainMessage = comment,
                    To = new List<string> { user.email },
                    UserName = user.userName,
                    SKU = sku,
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = changeSkuRequested;
                notificationModel.EmailContentModelType = "ChangeSkuRequestedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildChangeSkuRejectedTemplateForOrgUsers(Guid Id, string mfName, string requesitionNumber, string email)
        {
            //var changeSku = new ChangeSkuReject
            //{
            //    From = _configuration.GetValue<string>("NotificationFromEmail"),
            //    Subject = "Change SKU / Product Name request " + requesitionNumber + " is rejected by Tax Authority",
            //    MfName = mfName,
            //    RequesitionNumber = requesitionNumber,
            //    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
            //};
            //changeSku.To = new[] { email }.ToList();
            //NotificationModel notificationModel = new NotificationModel();
            //notificationModel.EmailContent = changeSku;
            //notificationModel.EmailContentModelType = notificationModel.EmailContent.GetType().Name;
            //notificationModel.NotificationType = NotificationType.Email;
            return null;
        }
        public List<NotificationModel> BuildChangeSkuApprovedTemplateForOrgUsers(Guid Id, int orgId, string requesitionNumber)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            foreach(var user in users)
            {
                var changeSkuapprove = new ChangeSkuApproved
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Your Change SKU request with " + requesitionNumber + " is Approved",
                    MfName = user.userName,
                    RequisitionNumber = requesitionNumber,
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
                };
                changeSkuapprove.To = new[] { user.email }.ToList();
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = changeSkuapprove;
                notificationModel.EmailContentModelType = "ChangeSkuApprovedForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            //return notificationModel;
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildChangeSkuRejectedTemplateForPortalAdmins(Guid Id, string mfName, string requesitionNumber, string email)
        {
            //var changeSku = new ChangeSkuReject
            //{
            //    From = _configuration.GetValue<string>("NotificationFromEmail"),
            //    Subject = "Change SKU / Product Name request " + requesitionNumber + " is rejected by Tax Authority",
            //    MfName = mfName,
            //    RequesitionNumber = requesitionNumber,
            //    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
            //};
            //changeSku.To = new[] { email }.ToList();
            //NotificationModel notificationModel = new NotificationModel();
            //notificationModel.EmailContent = changeSku;
            //notificationModel.EmailContentModelType = notificationModel.EmailContent.GetType().Name;
            //notificationModel.NotificationType = NotificationType.Email;
            //return notificationModel;
            return null;
        }
        public List<NotificationModel> BuildChangeSkuApprovedTemplateForPortalAdmins(Guid Id, string mfName, string requesitionNumber, string email)
        {
            //var changeSkuapprove = new ChangeSkuApprove
            //{
            //    From = _configuration.GetValue<string>("NotificationFromEmail"),
            //    Subject = "Change SKU / Product Name request " + requesitionNumber + " is Approved by Tax Authority",
            //    MfName = mfName,
            //    RequesitionNumber = requesitionNumber,
            //    URL = _configuration["HostingApp:Url"] + "OrderManagement/changeSkuProductNameView"
            //};
            //changeSkuapprove.To = new[] { email }.ToList();
            //NotificationModel notificationModel = new NotificationModel();
            //notificationModel.EmailContent = changeSkuapprove;
            //notificationModel.EmailContentModelType = notificationModel.EmailContent.GetType().Name;
            //notificationModel.NotificationType = NotificationType.Email;
            //return notificationModel;
            return null;
        }
        public List<NotificationModel> BuildOrderDeliverdTemplateForOrgUsers(Guid orderId, string orderNmbr, string DeliverAgentName, int orgId, string mfWareHouseInchargeName, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderDelivered = new OrderDelivered
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Order with number " + orderNmbr + " is delivered by " + DeliverAgentName,
                    UserName = user.userName,
                    OrderNumber = orderNmbr,
                    DeliveryAgentName = DeliverAgentName,
                    WarehouseInchargeName = mfWareHouseInchargeName,
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId
                };
                orderDelivered.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderDelivered;
                notificationModel.EmailContentModelType = "OrderDeliveredForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderDeliverdTemplateForPortalAdmins(Guid orderId, string orderNmbr, string DeliverAgentName, string orgName, string warehouseName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string> { "TaxAuthAdmin", "TaxAuthRevenueOfficer", "TsspAdmin", "TsspIntermediate", "TsspWarehouseIncharge" });
            foreach (var user in users)
            {
                var orderDelivered = new OrderDelivered
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = " Order with number " + orderNmbr + " of " + orgName + " is delivered by " + DeliverAgentName,
                    MfName = orgName,
                    UserName = user.userName,
                    OrderNumber = orderNmbr,
                    DeliveryAgentName = DeliverAgentName,
                    WarehouseInchargeName = warehouseName,
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId
                };
                orderDelivered.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderDelivered;
                notificationModel.EmailContentModelType = "OrderDeliveredForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildPickUpDeliveryTemplateForOrgUsers(Guid orderId, string orderNmbr, string DeliverAgentName, string mfWareHouseInchargeName, int orgId, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderPickedUp = new OrderPickedUp();
                orderPickedUp.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderPickedUp.Subject = "Order with number " + orderNmbr + " is picked up by " + DeliverAgentName;
                orderPickedUp.UserName = user.userName;
                orderPickedUp.OrderNumber = orderNmbr;
                orderPickedUp.DeliveryAgentName = DeliverAgentName;
                orderPickedUp.WarehouseInchargeName = mfWareHouseInchargeName;
                orderPickedUp.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderPickedUp.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderPickedUp;
                notificationModel.EmailContentModelType = "OrderPickedUpForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildPickupDeilveryTemplateForPortalAdmins(Guid orderId, string orderNmbr, string shipperName, string orgName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string> { "TaxAuthAdmin", "TaxAuthRevenueOfficer", "TsspAdmin", "TsspIntermediate", "TsspWarehouseIncharge" });
            foreach (var user in users)
            {
                var orderPickedUp = new OrderPickedUp
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Order with number " + orderNmbr + " is picked up by " + shipperName,
                    MfName = orgName,
                    OrderNumber = orderNmbr,
                    UserName = user.userName,
                    DeliveryAgentName = shipperName,
                    To = new List<string> { user.email },
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderPickedUp;
                notificationModel.EmailContentModelType = "OrderPickedUpForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderResubmitedTemplateForOrgAdmins(Guid orderId, string OrderNo, int orgId, string warehouseEmail)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            var currentWarehouseUser = users.Where(u => u.email == warehouseEmail).SingleOrDefault();
            var neededUsers = users.Where(u => u.Role != "MfWarehouseIncharge" && u.Role != "TpsafFacilityIncharge").ToList();
            neededUsers.Add(currentWarehouseUser);
            foreach (var user in neededUsers)
            {
                var orderResubmitted = new OrderResubmitted();
                orderResubmitted.From = _configuration.GetValue<string>("NotificationFromEmail");
                orderResubmitted.Subject = "Order with number " + OrderNo + " resubmitted";
                orderResubmitted.OrderNumber = OrderNo;
                orderResubmitted.URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderView/" + orderId;
                orderResubmitted.UserName = user.userName;
                orderResubmitted.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderResubmitted;
                notificationModel.EmailContentModelType = "OrderResubmittedForOrgUsers";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildOrderResubmitedTemplateForPortalAdmins(Guid orderId, string orderNmbr, string mfName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByRole(new List<string> { "TaxAuthAdmin", "TaxAuthRevenueOfficer" });
            foreach (var user in users)
            {
                var orderResubmitted = new OrderResubmitted
                {
                    From = _configuration.GetValue<string>("NotificationFromEmail"),
                    Subject = "Order with number " + orderNmbr + " is resubmitted by " + mfName,
                    MfName = mfName,
                    OrderNumber = orderNmbr,
                    UserName = user.userName,
                    To = new List<string> { user.email },
                    URL = _configuration["HostingApp:Url"] + "OrderManagement/OrderReview/" + orderId
                };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = orderResubmitted;
                notificationModel.EmailContentModelType = "OrderResubmittedForPortalAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildTemplateForInternalStockTranferRequested(Guid id, int orgId, string facilityName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            foreach (var user in users)
            {
                var internalStock = new InternalStockTransferRequested();
                internalStock.From = _configuration.GetValue<string>("NotificationFromEmail");
                internalStock.Subject = facilityName + " raised a request for Internal Stock Transfer";
                internalStock.URL = _configuration["HostingApp:Url"] + "OrderManagement/StockRequestDetail/" + id;
                internalStock.FacilityName = facilityName;
                internalStock.UserName = user.userName;
                internalStock.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = internalStock;
                notificationModel.EmailContentModelType = "InternalStockTranferRequested";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildInternalStockTraferApprovedTemplateForRequestorAndTpsafAdmin(Guid id, int orgId, int requestorId, string facilityName, string approverName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgIdandOrgRole(orgId, "TpsafAdmin");
            var requestor = GetUserEmailIdByUserId(requestorId);
            users.Add(requestor);
            foreach(var user in users)
            {
                var stockApproved = new InternalStockTransferApproved();
                stockApproved.From = _configuration.GetValue<string>("NotificationFromEmail");
                stockApproved.Subject = "Internal Stock Request is approved by " + facilityName ;
                stockApproved.URL = _configuration["HostingApp:Url"] + "OrderManagement/StockRequestDetail/" + id;
                stockApproved.FacilityName = facilityName;
                stockApproved.ApproverName = approverName;
                stockApproved.UserName = user.userName;
                stockApproved.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = stockApproved;
                notificationModel.EmailContentModelType = "InternalStockTransferApprovedForTpsafAdminAndRequestor";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public NotificationModel BuildInternalStockTraferApprovedTemplateForApprover(Guid id, int userId)
        {
            var user = GetUserEmailIdByUserId(userId);
            var stockApproved = new InternalStockTransferApproved();
            stockApproved.From = _configuration.GetValue<string>("NotificationFromEmail");
            stockApproved.Subject = "Fulfill the approved Internal Stock Transfer Request ";
            stockApproved.URL = _configuration["HostingApp:Url"] + "OrderManagement/StockRequestFulfill";
            stockApproved.UserName = user.UserName;
            stockApproved.To = new List<string> { user.EmailId };
            NotificationModel notificationModel = new NotificationModel();
            notificationModel.EmailContent = stockApproved;
            notificationModel.EmailContentModelType = "InternalStockTransferApprovedForApprover";
            notificationModel.NotificationType = NotificationType.Email;
            return notificationModel;
        }
        public List<NotificationModel> BuildInternalStockTraferFilfilledTemplate(Guid id, int orgId, int requestorId, string facilityName, string approverName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgIdandOrgRole(orgId, "TpsafAdmin");
            var requestor = GetUserEmailIdByUserId(requestorId);
            users.Add(requestor);
            foreach (var user in users)
            {
                var stockFulFilled = new InternalStockTransferFulFilled();
                stockFulFilled.From = _configuration.GetValue<string>("NotificationFromEmail");
                stockFulFilled.Subject = "Internal Stock Request is fulfilled by " + facilityName;
                stockFulFilled.URL = _configuration["HostingApp:Url"] + "OrderManagement/StockRequestDetail/" + id;
                stockFulFilled.FacilityName = facilityName;
                stockFulFilled.ApproverName = approverName;
                stockFulFilled.UserName = user.userName;
                stockFulFilled.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = stockFulFilled;
                notificationModel.EmailContentModelType = "InternalStockTransferFulFilled";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }

        public List<NotificationModel> BuildPrintOrderPlacedTemplateForAdmins(/*Guid printOrderId,*/int orgId, string prtNumber)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            foreach (var user in users)
            {
                var printOrder = new PrintOrderSubmitted();
                printOrder.From = _configuration.GetValue<string>("NotificationFromEmail");
                printOrder.Subject = "Print order placed successfully";
                printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrders";
                //printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrderView/" + printOrderId;
                printOrder.UserName = user.userName;
                printOrder.OrderNumber = prtNumber;
                printOrder.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = printOrder;
                notificationModel.EmailContentModelType = "PrintOrderSubmitted";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildPrintOrderPlacedTemplateForPrintPartner(/*Guid printOrderId,*/ string prtNumber, int printPartnerId)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(printPartnerId);
            foreach (var user in users)
            {
                var printOrder = new PrintOrderSubmitted();
                printOrder.From = _configuration.GetValue<string>("NotificationFromEmail");
                printOrder.Subject = "Print order " + prtNumber + " placed successfully";
                //printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrderView/" + printOrderId;
                printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrders";
                printOrder.UserName = user.userName;
                printOrder.OrderNumber = prtNumber;
                printOrder.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = printOrder;
                notificationModel.EmailContentModelType = "PrintOrderSubmittedForPrintPartner";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildPrintOrderApprovedTemplateForAdmins(Guid printOrderId, string prtNumber,  int orgId, string printPartnerName)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(orgId);
            foreach (var user in users)
            {
                var printOrder = new PrintOrderApproved();
                printOrder.From = _configuration.GetValue<string>("NotificationFromEmail");
                printOrder.Subject = "Print order with number " + prtNumber + " is approved by " + printPartnerName;
                printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrderView/" + printOrderId;
                printOrder.UserName = user.userName;
                printOrder.OrderNumber = prtNumber;
                printOrder.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = printOrder;
                notificationModel.EmailContentModelType = "PrintOrderApprovedForAdmins";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public List<NotificationModel> BuildPrintOrderClosedTemplate(Guid printOrderId, string prtNumber, int pritPartnerId)
        {
            var lstNotificationModel = new List<NotificationModel>();
            var users = GetUsersEmailIdByOrgId(pritPartnerId);
            foreach (var user in users)
            {
                var printOrder = new PrintOrderClosed();
                printOrder.From = _configuration.GetValue<string>("NotificationFromEmail");
                printOrder.Subject = prtNumber + " is delivered ";
                printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrderView/" + printOrderId;
                printOrder.UserName = user.userName;
                printOrder.OrderNumber = prtNumber;
                printOrder.To = new List<string> { user.email };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = printOrder;
                notificationModel.EmailContentModelType = "PrintOrderClosed";
                notificationModel.NotificationType = NotificationType.Email;
                lstNotificationModel.Add(notificationModel);
            }
            return lstNotificationModel;
        }
        public NotificationModel BuildPrintOrderRejectedTemplate(Guid printOrderId,string comments, string prtNumber, int userId, string printPartnerName)
        {
            var user = GetUserEmailIdByUserId(userId);
                var printOrder = new PrintOrderRejected();
                printOrder.From = _configuration.GetValue<string>("NotificationFromEmail");
                printOrder.Subject = prtNumber + " is rejected by " + printPartnerName;
                printOrder.URL = _configuration["HostingApp:Url"] + "OrderManagement/PrintOrderView/" + printOrderId;
                printOrder.UserName = user.UserName;
                printOrder.PrintPartnerName = printPartnerName;
            printOrder.RejectedComment = comments;
                printOrder.OrderNumber = prtNumber;
                printOrder.To = new List<string> { user.EmailId };
                NotificationModel notificationModel = new NotificationModel();
                notificationModel.EmailContent = printOrder;
                notificationModel.EmailContentModelType = "PrintOrderRejected";
                notificationModel.NotificationType = NotificationType.Email;
            return notificationModel;
        }
        private List<(string userName, string email)> GetUsersEmailIdByRole(List<string> roleList)
        {
            var task = Task.Run(async () => await _userClient.GetUsersEmailbyRole(roleList));
            return task.Result;
        }
        private List<(string userName, string Role, string email)> GetUsersEmailIdByOrgId(int OrgId)
        {
            var task = Task.Run(async () => await _userClient.GetUsersEmailbyOrgId(OrgId));
            return task.Result;
        }
        private List<(string userName,string role,string email)> GetUsersEmailIdByOrgIdandOrgRole(int OrgId, string role)
        {
            var task = Task.Run(async () => await _userClient.GetUsersEmailbyOrgIdandOrgRole(OrgId, role));
            return task.Result;
        }
        private (string UserName, string Role, string EmailId) GetUserEmailIdByUserId(int userId)
        {
            var task = Task.Run(async () => await _userClient.GetEmailByUserId(userId));
            return task.Result;
        }
    }
}
