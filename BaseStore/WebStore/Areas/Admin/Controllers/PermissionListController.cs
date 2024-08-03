﻿using Data.MasterInterface;
using Domain.User.Permission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Domain;
using Domain.User;
using Core.Interface.Admin;

namespace NoorMehr.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PermissionListController : Controller
    {
        private IPermisionList _permisionList;

        public PermissionListController(IPermisionList permisionList)
        {
            _permisionList = permisionList;
        }
        [HttpGet]
        public IActionResult ShowPermision(string MyArea=null, string SearchController = null)
        {
            var first = _permisionList.GetAll().Any();
            if (!first)
            {
                return RedirectToAction("insertArea");
            }
            else
            {
                SearchController = SearchController == "-1" ? null : SearchController;
                ViewBag.Area = new SelectList(_permisionList.GetAllArea(), "Value", "Text", MyArea);
             
                if (MyArea == null & SearchController == null)
                {
                    ViewBag.Controller = new SelectList(_permisionList.GetControllerByArea(MyArea), "Value", "Text", SearchController);
                    var obj = _permisionList.GetAll();
                    return View(obj);
                }
                else
                {
                    var obj = _permisionList.GetPermisionByAreaAndController(MyArea, SearchController);
                    return View(obj);
                }
            }
            
           
        }
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var result = _permisionList.GetById(Id);
            ViewBag.ParentList = new SelectList(_permisionList.GetParentList(), "PermissionListId", "Descript");
            return PartialView("Edit",result);
        }
        [HttpPost]
        public IActionResult Edit(PermissionList permissionList)
        {
            if (!ModelState.IsValid)
            {
                return View(permissionList);
            }
            var obj = _permisionList.GetById(permissionList.PermissionListId);
            obj.ParentId = permissionList.ParentId!=-1?permissionList.ParentId:obj.ParentId;
            obj.Radif=permissionList.Radif;
            obj.Descript=permissionList.Descript;
            obj.Status=permissionList.Status;
            _permisionList.Update(obj);
            return RedirectToAction("ShowPermision",new { MyArea =obj.Area, SearchController =obj.ControllerName});
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            if (Id == null)
            {
                return BadRequest();
            }
            var result = _permisionList.GetById(Id);
            if (result == null)
            {
                return NotFound();
            }

            _permisionList.Delete(result);
            return RedirectToAction("ShowPermision");
        }



        [HttpGet]
        public ActionResult GetController(string MyArea)
        {
            var obj = new SelectList(_permisionList.GetControllerByArea(MyArea), "ControllerName", "ControllerName");
            return Json(obj);
        }
        public IActionResult insertArea()
        {
            var ali = ActionAndControllerNamesList();
         



            foreach (var item in ali)
            {


                int AreaId = 0;
                if (item.Area != null)
                {
                    if (_permisionList.checkExistArea(item.Area) == 0)
                    {
                        _permisionList.Insert(new PermissionList()
                        {
                            Area = item.Area,
                            ControllerName = null,
                            ActionName = null,
                            ParentId = null,
                            Descript = null,
                            Status = 0
                        });

                        AreaId = _permisionList.checkExistArea(item.Area);
                    }
                    else
                    {
                        AreaId = _permisionList.checkExistArea(item.Area);
                    }
                }
                else
                {
                    AreaId = _permisionList.checkExistArea(item.Area);
                }


                if (_permisionList.checkExistController(item.Area, item.Controller) == 0)
                {
                    var menu = new PermissionList()
                    {
                        Area = item.Area,
                        ControllerName = item.Controller,
                        ActionName = null,
                        ParentId = AreaId,
                        Descript = null,
                        Status = null

                    };
                    _permisionList.Insert(menu);
                    AreaId = _permisionList.checkExistController(item.Area, item.Controller);
                }
                else
                {
                    AreaId = _permisionList.checkExistController(item.Area, item.Controller);
                }
                if (!_permisionList.CheckExistPermission(item.Area, item.Controller, item.Action))
                {
                    var menu = new PermissionList()
                    {
                        Area = item.Area,
                        ControllerName = item.Controller,
                        ActionName = item.Action,
                        ParentId = AreaId,
                        Descript = null,
                        Status = null

                    };
                    _permisionList.Insert(menu);
                }

            }
            return RedirectToAction("ShowPermision", _permisionList.GetPermisionByAreaAndController(null,null));
        }
        public IList<ControllerActions> ActionAndControllerNamesList()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var controlleractionlist = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Select(x => new
                {
                    Controller = x.DeclaringType.Name,
                    Action = x.Name,
                    Area = x.DeclaringType.CustomAttributes.Where(c => c.AttributeType == typeof(AreaAttribute))

                }).ToList();
            var list = new List<ControllerActions>();
            foreach (var item in controlleractionlist)
            {
                if (item.Area.Count() != 0)
                {
                    list.Add(new ControllerActions()
                    {
                        Controller = item.Controller.Replace("Controller", null),
                        Action = item.Action,
                        Area = item.Area.Select(v => v.ConstructorArguments[0].Value.ToString()).FirstOrDefault()
                    });
                }
                else
                {
                    list.Add(new ControllerActions()
                    {
                        Controller = item.Controller.Replace("Controller", null),
                        Action = item.Action,
                        Area = null,
                        
                    });
                }
            }

            return list;
        }
    }
}
