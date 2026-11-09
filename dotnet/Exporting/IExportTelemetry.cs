namespace WishList.Exporting
{
    public interface IExportTelemetry
    {
        void Record(ExportObservation observation);
    }
}
