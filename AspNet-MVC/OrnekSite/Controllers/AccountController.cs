﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OrnekSite.Entity;
using OrnekSite.Identity;
using OrnekSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OrnekSite.Controllers
{
    public class AccountController : Controller
    {
        DataContext db = new DataContext(); //veritabanı ile bağlantı kurulur 
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<ApplicationRole> RoleManager;
        public AccountController()
        {
            var userStore = new UserStore<ApplicationUser>(new IdentityDataContext()); //veri tabanıylada ilişkili olarak kullanıcıları yönetme işlmeidir
            UserManager = new UserManager<ApplicationUser>(userStore); // kullanıcıları tanımış ve yönetmiş oluyoruz

            var roleStore = new RoleStore<ApplicationRole>(new IdentityDataContext());
            RoleManager = new RoleManager<ApplicationRole>(roleStore);
        }
        // GET: Account
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost] //şifre değiştirme butonuna basıldığında gerçekleşek olaylar buraya yazılır
        [Authorize] //sisteme giriş yapanlar bu sayfaya gelebilir demek
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid) //kullanıcı zorunlu alanları girmiş mi kontrol eder
            {
                var result = UserManager.ChangePassword(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                return View("Update");
            }
            return View(model);
        }
        public PartialViewResult UserCount()
        {
            var u = UserManager.Users;
            return PartialView(u);
        }
        public PartialViewResult UserList()
        {
            var u = UserManager.Users;
            return PartialView(u);
        }
        public ActionResult UserProfil()
        {
            var id = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId(); //giriş yapan bağlı olan kullanıcının idsini verir
            var user = UserManager.FindById(id); //kullancı bulunur
            var data =new UserProfile()
                {
                Id=user.Id ,
                Name =user.Name ,
                Surname =user.Surname ,
                Email =user.Email ,
                Username =user.UserName 
            };
            return View(data);
        }
        [HttpPost]
        public ActionResult UserProfile(UserProfile model)
        {
            var user = UserManager.FindById(model.Id);
            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.UserName = model.Username;
            UserManager.Update(user);
            return View("Update");
        }

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult LogOut() //kullanıcı sayfadan çıkış yapsın
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Index" ,"Home");
        }
        [HttpPost]
        public ActionResult Login( Login model ,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.Find(model.Username, model.Password); // girilene göre bir kullanıcı bulacak
                if (user!=null) //Kullanıcı sistemde var mı yok mu kontrol eder
                {
                    var authManager = HttpContext.GetOwinContext().Authentication;
                    var Identityclaims = UserManager.CreateIdentity(user, "ApplicationCookie");
                    var authProperties = new AuthenticationProperties();
                    authProperties.IsPersistent = model.RememberMe;
                    authManager.SignIn(authProperties, Identityclaims); //sistem beni hatırla
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else //girişi olmayan kullanıcı giriş yaparsa 
                {
                    ModelState.AddModelError("LoginUserError", "Böyle bir Kullanıcı Yok...");
                }
            }
            return View(model);
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
       public ActionResult Register(Register model) //register klasındaki özelliklere ulaşırız
        {
            if (ModelState.IsValid) //kullanıcı her şeyi girip doğru yaptıysa kayıt işlemi gerçekleşsin
            {
                var user = new ApplicationUser();
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.Email = model.Email;
                user.UserName = model.Username;
                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    if (RoleManager.RoleExists("user"))
                    {
                        UserManager.AddToRole(user.Id, "user");
                    }
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("RegisterUserError", "Kullanıcı Oluşturma Hatası...");
                }
            }
            return View(model);
        }
        public ActionResult Index()
        {
            var username = User.Identity.Name;  //kullanıcı kullanıcı adına bastığında ındex sayfası gelir
            var orders = db.Orders.Where(i => i.UserName == username).Select(i => new UserOrder  //veritabanına kullanıcın sistemde yapmış olduğu siparişleri getirir uani filtreleme işlemi
            {
                Id = i.Id,
                OrderNumber = i.OrderNumber,
                OrderState = i.OrderState,
                OrderDate = i.OrderDate,
                Total = i.Total
            }).OrderByDescending(i => i.OrderDate).ToList(); //tarihe göre sıralama yapılır          
            return View(orders);
        }
        public ActionResult Details (int id)
        {
            var model = db.Orders.Where(i => i.Id == id).Select(i => new OrderDetails()
            {
                OrderId = i.Id,
                OrderNumber = i.OrderNumber,
                Total = i.Total,
                OrderDate = i.OrderDate,
                OrderState = i.OrderState,
                Adres = i.Adres,
                Sehir = i.Sehir,
                Semt = i.Semt,
                Mahalle = i.Mahalle,
                PostaKodu = i.PostaKodu,
                OrderLines = i.OrdeLines.Select(x => new OrderLineModel()
                {
                    ProductId = x.ProductId,
                    Image = x.Product.Image,
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity,
                    Price = x.Price,

                }).ToList()   //tek bir siparişte birden fazla ürün sipariş edilebilir
            }).FirstOrDefault();  //tek bir sipariş 
           return View(model);
        }
    }
}