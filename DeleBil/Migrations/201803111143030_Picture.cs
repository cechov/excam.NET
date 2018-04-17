namespace DeleBil.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Picture : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Pictures", "ContentType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Pictures", "ContentType", c => c.String());
        }
    }
}
