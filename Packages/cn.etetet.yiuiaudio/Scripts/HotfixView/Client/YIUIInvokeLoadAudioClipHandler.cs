using UnityEngine;

namespace ET.Client
{
    [Invoke(EYIUIInvokeType.Sync)]
    public class YIUIInvokeLoadAudioClipSyncHandler : AInvokeEntityHandler<YIUIInvokeLoadAudioClip, AudioClip>
    {
        public override AudioClip Handle(Entity entity, YIUIInvokeLoadAudioClip args)
        {
            var resName = args.ResName;
            EntityRef<YIUILoadComponent> yiuiLoad = entity.YIUILoad();
            if (yiuiLoad.Entity == null) return null;

            #if UNITY_EDITOR
            if (!yiuiLoad.Entity.VerifyAssetValidity(resName))
            {
                Log.Error($"验证资产有效性 没有这个资源 无法加载 请检查 {resName}");
                return null;
            }
            #endif

            var audioClip = yiuiLoad.Entity.LoadAsset<AudioClip>(resName);

            if (audioClip == null)
            {
                Log.Error($"加载失败 没有这个资源 无法加载 请检查 {resName}");
                return null;
            }

            return audioClip;
        }
    }

    [Invoke(EYIUIInvokeType.Async)]
    public class YIUIInvokeLoadAudioClipAsyncHandler : AInvokeEntityHandler<YIUIInvokeLoadAudioClip, ETTask<AudioClip>>
    {
        public override async ETTask<AudioClip> Handle(Entity entity, YIUIInvokeLoadAudioClip args)
        {
            EntityRef<YIUILoadComponent> yiuiLoad = entity.YIUILoad();

            if (yiuiLoad.Entity == null) return null;
            var resName = args.ResName;
            #if UNITY_EDITOR
            if (!yiuiLoad.Entity.VerifyAssetValidity(resName))
            {
                Log.Error($"验证资产有效性 没有这个资源 无法加载 请检查 {resName}");
                return null;
            }
            #endif

            var audioClip = await yiuiLoad.Entity.LoadAssetAsync<AudioClip>(resName);

            if (audioClip == null)
            {
                Log.Error($"加载失败 没有这个资源 无法加载 请检查 {resName}");
                return null;
            }

            return audioClip;
        }
    }
}