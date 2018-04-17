namespace DeleBil.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cars",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LicensePlate = c.String(nullable: false),
                        Title = c.String(nullable: false),
                        Owner_Id = c.String(nullable: false, maxLength: 128),
                        Picture_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Owner_Id, cascadeDelete: true)
                .ForeignKey("dbo.Pictures", t => t.Picture_Id, cascadeDelete: true)
                .Index(t => t.Owner_Id)
                .Index(t => t.Picture_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Pictures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContentType = c.String(),
                        PictureData = c.Binary(),
                        Parking_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Parkings", t => t.Parking_Id)
                .Index(t => t.Parking_Id);
            
            CreateTable(
                "dbo.LeaseHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateLeased = c.DateTime(nullable: false),
                        DateDelivered = c.DateTime(nullable: false),
                        Lease_Id = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Leases", t => t.Lease_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.Lease_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Leases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        Car_Id = c.Int(),
                        DeliveryLocation_Id = c.Int(),
                        Leaser_Id = c.String(maxLength: 128),
                        PickupLocation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cars", t => t.Car_Id)
                .ForeignKey("dbo.Parkings", t => t.DeliveryLocation_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Leaser_Id)
                .ForeignKey("dbo.Parkings", t => t.PickupLocation_Id)
                .Index(t => t.Car_Id)
                .Index(t => t.DeliveryLocation_Id)
                .Index(t => t.Leaser_Id)
                .Index(t => t.PickupLocation_Id);
            
            CreateTable(
                "dbo.Parkings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Location = c.Geography(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.LeaseHistories", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.LeaseHistories", "Lease_Id", "dbo.Leases");
            DropForeignKey("dbo.Leases", "PickupLocation_Id", "dbo.Parkings");
            DropForeignKey("dbo.Leases", "Leaser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Leases", "DeliveryLocation_Id", "dbo.Parkings");
            DropForeignKey("dbo.Pictures", "Parking_Id", "dbo.Parkings");
            DropForeignKey("dbo.Leases", "Car_Id", "dbo.Cars");
            DropForeignKey("dbo.Cars", "Picture_Id", "dbo.Pictures");
            DropForeignKey("dbo.Cars", "Owner_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Leases", new[] { "PickupLocation_Id" });
            DropIndex("dbo.Leases", new[] { "Leaser_Id" });
            DropIndex("dbo.Leases", new[] { "DeliveryLocation_Id" });
            DropIndex("dbo.Leases", new[] { "Car_Id" });
            DropIndex("dbo.LeaseHistories", new[] { "User_Id" });
            DropIndex("dbo.LeaseHistories", new[] { "Lease_Id" });
            DropIndex("dbo.Pictures", new[] { "Parking_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Cars", new[] { "Picture_Id" });
            DropIndex("dbo.Cars", new[] { "Owner_Id" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Parkings");
            DropTable("dbo.Leases");
            DropTable("dbo.LeaseHistories");
            DropTable("dbo.Pictures");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Cars");
        }
    }
}
