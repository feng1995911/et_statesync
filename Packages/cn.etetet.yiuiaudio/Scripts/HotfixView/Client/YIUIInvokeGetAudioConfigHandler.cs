using YIUIFramework;

namespace ET.Client
{
    [Invoke]
    public class YIUIInvokeGetAudioConfigHandler : AInvokeHandler<YIUIInvokeGetAudioConfig, IAudioConfig>
    {
        public override IAudioConfig Handle(YIUIInvokeGetAudioConfig args)
        {
            return AudioConfigCategory.Instance?.GetOrDefault(args.AudioName);
        }
    }
}
