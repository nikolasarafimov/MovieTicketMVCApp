namespace MovieTicketMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastChanges : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tickets", "TotalRows");
            DropColumn("dbo.Tickets", "TotalCols");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "TotalCols", c => c.Int(nullable: false));
            AddColumn("dbo.Tickets", "TotalRows", c => c.Int(nullable: false));
        }
    }
}
