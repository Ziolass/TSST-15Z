using System;

namespace NetworkNode.SDHFrame
{
    public interface IFrameBuilder
    {
        IFrame BuildFrame(String textFrame);

        String BuildLiteral(IFrame textFrame);
    }
}