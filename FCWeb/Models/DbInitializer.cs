using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FCWeb.Models
{
    public class DbInitializer:DropCreateDatabaseIfModelChanges<FCDbContext>
    {
        protected override void Seed(FCDbContext context)
        {
            context.Permission.AddRange(new List<Permissions> {

                new Permissions
                {
                    ID=1,
                    PermissionName="球队队长"
                },
                new Permissions
                {
                    ID=2,
                    PermissionName="添加赛程"
                },
                 new Permissions
                {
                    ID=3,
                    PermissionName="赛事添加球员"
                },
                  new Permissions
                {
                    ID=4,
                    PermissionName="添加本地球员"
                },
                  new Permissions
                {
                    ID=5,
                    PermissionName="球员申请"
                },
                  new Permissions
                {
                    ID=6,
                    PermissionName="移除球员"
                },
                  new Permissions
                {
                    ID=7,
                    PermissionName="修改球队信息"
                },

            });
            context.SaveChanges();
            base.Seed(context);
        }
    }
}