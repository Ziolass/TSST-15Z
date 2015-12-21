using System;

namespace NetworkNode.SDHFrame
{
    public interface IFrameBuilder
    {
        IFrame BuildFrame(String textFrame);
        IFrame BuildEmptyFrame();
        String BuildLiteral(IFrame textFrame);
    }
}