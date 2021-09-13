using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HTWGame.Domain
{
	public class HuntTheWumpusFactory
	{
		public static IHuntTheWumpus MakeGame(string className, IHuntTheWumpusMessageReceiver receiver)
		{
			IHuntTheWumpus huntTheWumpus = null;
			try
			{
				Type htwClass = Type.GetType(className);
				ConstructorInfo htwClassConstructor = htwClass.GetConstructor(new Type[] { typeof(IHuntTheWumpusMessageReceiver) });
				huntTheWumpus = (IHuntTheWumpus)htwClassConstructor.Invoke(new object[] { receiver });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Environment.Exit(-1);
			}
			return huntTheWumpus;
		}
	}
}
