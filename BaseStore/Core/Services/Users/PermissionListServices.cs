﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Core.Dto.ViewModel.Admin.Role;
using Core.Enums;
using Core.Interface.Admin;
using Dapper;
using Data.MasterInterface;
using Domain;
using Domain.User;
using Domain.User.Permission;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core.Services.Users
{
    public class PermissionListServices:IPermisionList
    {
        IMaster<PermissionList> _master;
        IMaster<RolePermission> _RolePemissionmaster;

        public PermissionListServices(IMaster<PermissionList> master,IMaster<RolePermission> rolePemissionmaster)
        {
            _master = master;
            _RolePemissionmaster=rolePemissionmaster;
        }

        public IEnumerable<PermissionList> GetAll()
        {
            return _master.GetAll();
        }

        public IEnumerable<PermissionList> GetPermisionByAreaAndController(string Area, string Controller)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("Area", Area, DbType.String);
            p.Add("Controller", Controller, DbType.String);
            return _master.GetAll("SearchPermission", p);
        }

        public IEnumerable<PermissionList> GetAllArea()
        {
            DynamicParameters p = new DynamicParameters();
            return _master.GetAll("GetAllArea", p);
        }

        public IEnumerable<PermissionList> GetControllerByArea(string Area)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("Area", Area, DbType.String);
            return _master.GetAll("GetControllerByArea", p);

        }

        public List<PermissionList> UserMenu()
        {
            return _master.GetAll(a=>a.Status==2).ToList();
        }

        public int checkExistArea(string Area)
        {
            if (_master.GetAll(a => a.Area == Area && a.Area != null && a.ActionName == null && a.ControllerName == null).Any())
                return _master.GetAll(a => a.Area == Area && a.Area != null && a.ActionName == null && a.ControllerName == null).FirstOrDefault().PermissionListId;
            else
                return 0;
        }

        public int checkExistController(string Area, string Controller)
        {
            if (_master.GetAll( a=> a.Area == Area && a.ControllerName == Controller && a.ActionName == null).Any())
                return _master.GetAll( a=> a.Area == Area && a.ControllerName == Controller && a.ActionName == null).FirstOrDefault().PermissionListId;
            else
                return 0;
        }

        public bool CheckExistPermission(string Area, string Controller, string Action)
        {
            var t = _master.GetAll(a => a.Area == Area & a.ControllerName == Controller & a.ActionName == Action).Any();
            return t;
        }

    

        public PermissionList Insert(PermissionList permissionList)
        {
            return _master.Insert(permissionList);
        }

        public PermissionList GetById(int PermissionId)
        {
            return _master.GetAll(a => a.PermissionListId == PermissionId).FirstOrDefault();
        }

        public PermissionList Update(PermissionList permissionList)
        {
            return _master.Update(permissionList);
        }

        public IEnumerable<PermissionList> GetParentList()
        {
            return _master.GetAll(a => a.ActionName == null&a.Status==2);
        }

        public bool Delete(PermissionList permissionList)
        {
            return _master.Delete(permissionList);
        }

        public IEnumerable<PermissionList> permissionLists()
        {
            var obj = _master.GetAll(a => a.Status != (int)PermissionStatus.menu);
            return obj;
        }

        public IEnumerable<ShowMenuVm> GetAllMenu()
        {
            return _master.GetAll(a => a.Status == 2).Select(a => new ShowMenuVm()
                { PermissionListId = a.PermissionListId, MenuDesc = a.Descript,ParentId = a.ParentId});
        }

        public IEnumerable<RolePermission> GetPermissionOfRole(int RoleId)
        {
            return _RolePemissionmaster.GetAll(a => a.RoleId == RoleId);
        }

        public IEnumerable<SelectListItem> GetContrllersOfArea(int SystemMenuId)
        {
            var area = _master.GetAll(a => a.ParentId == SystemMenuId).FirstOrDefault()?.Area;
            var obj = _master.GetAll(a => a.Area == area & a.ActionName == null).Select(a => new SelectListItem()
            {
                Text = a.ControllerName,
                Value = a.PermissionListId.ToString()
            }).AsEnumerable().Append(new SelectListItem("منو", "-1")).Append(new SelectListItem("کنترلر اصلی", "-2"));
            return obj;
        }

        public IEnumerable<SelectListItem> ParentList()
        {
            var obj = _master.GetAll(a => a.ParentId == null).Select(a => new SelectListItem()
            {
                Text = a.Descript,
                Value = a.PermissionListId.ToString()
            });
            var dis = obj.OrderBy(a => a.Text);
            return dis;
        }

        public IEnumerable<PermissionList> GetAllParentPermissionList()
        {
            return _master.GetAll(a => a.ParentId == (int)MenuStatus.permission);
        }

        public IEnumerable<SelectListItem> PermissionParentList()
        {
            var obj = _master.GetAll(a => a.ParentId == 0).Select(a => new SelectListItem()
            {
                Text = a.Descript,
                Value = a.PermissionListId.ToString()
            }).Append(new SelectListItem("دسترسی والد", "0"));
            var dis = obj.OrderBy(a => a.Text);
            return dis;
        }
    }
}
