using Cassandra.Mapping;
namespace Tarzan.Nfx.Model
{
    public partial class AffObject
    {
        public static Map<AffObject> Mapping =>
            new Map<AffObject>()
            .TableName("catalogue")
            .PartitionKey(x => x.ObjectName)
            .Column(c => c.__isset, cc => cc.Ignore())
            .Column(c => c.ObjectType, cc => cc.WithSecondaryIndex());
    }

    public partial class AffStatement
    {
        public static Map<AffStatement> Mapping =>
           new Map<AffStatement>()
           .TableName("relations")
           .PartitionKey(x => x.Subject)
           .Column(f => f.__isset, cc => cc.Ignore());
    }
}
