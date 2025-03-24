using Mapster;
using ServiceX.Contracts.Order;
using ServiceX.Entites;

namespace ServiceX.Mapping;
public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Configuration
        config.NewConfig<Order, OrderResponset>()
            .Map(dest => dest.Id, src => src.OrderId)
            .Map(dest => dest.UserName, src => src.Customer.User.FirstName)
            .Map(dest => dest.TechnicianName, src => $"{src.Technician.User.FirstName} {src.Technician.User.LastName}")
            .Map(dest => dest.ServiceName, src => src.Technician.Service!.Name)
            .Map(dest => dest.OrderStatus, src => src.Status)
            .Map(dest => dest.problemDescription, src => src.ProblemDescription);
        #endregion
    }
}
