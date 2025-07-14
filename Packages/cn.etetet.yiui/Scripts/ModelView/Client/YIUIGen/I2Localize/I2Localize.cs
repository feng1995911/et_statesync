using I2.Loc;

namespace ET.Client
{
	public static class I2Localize
	{

		/// <summary>
		/// 无
		/// </summary>
		[StaticField]
		public static string None 		{ get{ return LocalizationManager.GetTranslation (I2Terms.None); } }
		/// <summary>
		/// 中文
		/// </summary>
		[StaticField]
		public static string Test 		{ get{ return LocalizationManager.GetTranslation (I2Terms.Test); } }
		/// <summary>
		/// 中文填充{0}
		/// </summary>
		[StaticField]
		public static string TestFormat 		{ get{ return LocalizationManager.GetTranslation (I2Terms.TestFormat); } }
		/// <summary>
		/// 登录
		/// </summary>
		[StaticField]
		public static string Login 		{ get{ return LocalizationManager.GetTranslation (I2Terms.Login); } }
	}
}
