using System;
using InstantGamesBridge.Common;

namespace InstantGamesBridge.Modules.Advertisement
{
    [Serializable]
    public class ShowBannerVkOptions : ShowBannerPlatformDependedOptions
    {
        public VkBannerPosition position;

        public ShowBannerVkOptions(VkBannerPosition position)
        {
            _targetPlatform = OptionsTargetPlatform.VK;
            this.position = position;
        }

        public new string ToJson()
        {
            return position.ToString().ToLower().SurroundWithKey("position")
                .SurroundWithKey(_targetPlatform.ToString().ToLower())
                .SurroundWithBraces();
        }
    }
}