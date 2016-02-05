namespace ConectionController.Communication.ReqResp
{
    public enum CommunicationType
    {
        CC_COMMUNICATION
    }

    public class CcResponse
    {
        public string Type { get; set; }
        public string Response { get; set; }

        public CcResponse()
        {
            Type = CommunicationType.CC_COMMUNICATION.ToString();
        }
    }
}