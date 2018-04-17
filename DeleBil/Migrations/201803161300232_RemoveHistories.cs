namespace DeleBil.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveHistories : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LeaseHistories", "Lease_Id", "dbo.Leases");
            DropForeignKey("dbo.LeaseHistories", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.LeaseHistories", new[] { "Lease_Id" });
            DropIndex("dbo.LeaseHistories", new[] { "User_Id" });
            DropTable("dbo.LeaseHistories");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.LeaseHistories", "User_Id");
            CreateIndex("dbo.LeaseHistories", "Lease_Id");
            AddForeignKey("dbo.LeaseHistories", "User_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.LeaseHistories", "Lease_Id", "dbo.Leases", "Id");
        }
    }
}
