namespace CheckCisService.Models.Enums
{
    public enum CheckCisServiceStatus
    {
        /// <summary>
        /// �������� ������ �������.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// ������ ������-��������.
        /// </summary>
        OnlineCheckFailed = 1,
        /// <summary>
        /// ������ ������-��������.
        /// </summary>
        OfflineCheckFailed = 2,
        /// <summary>
        /// ������ ����� ��������.
        /// </summary>
        AllChecksFailed = 3,
    }
}
