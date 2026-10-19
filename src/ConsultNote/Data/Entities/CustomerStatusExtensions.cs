namespace ConsultNote.Data.Entities;

public static class CustomerStatusExtensions
{
    public static bool IsArchived(this CustomerStatus status)
    {
        return status is CustomerStatus.Discarded or CustomerStatus.Delivered;
    }
}
