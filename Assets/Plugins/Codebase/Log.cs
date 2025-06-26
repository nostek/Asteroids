using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

public static class Log
{
	private static StringBuilder _sb;
	private static StringBuilder Sb => _sb ??= new StringBuilder(1000);

	public static void Time(string actionName, Action action)
	{
		var sw = new Stopwatch();
		sw.Start();
		action();
		sw.Stop();
		D(actionName, "=>", sw.ElapsedMilliseconds, "ms");
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void T(params object[] vars)
	{
		if (vars.Length / 2 % 1 != 0)
			throw new Exception("Parameters must be key then value. Dividable by two.");

		Sb.Clear();

		var maxLength = 0;
		for (var i = 0; i < vars.Length; i += 2)
			maxLength = Math.Max(maxLength, vars[i] == null ? 0 : vars[i].ToString().Length);

		var maxLength2 = 0;
		for (var i = 1; i < vars.Length; i += 2)
			maxLength2 = Math.Max(maxLength2, vars[i] == null ? 0 : vars[i].ToString().Length);

		Sb.AppendFormat("{0}\n", new string('-', maxLength + maxLength2 + 7));

		for (var i = 0; i < vars.Length; i += 2)
			Sb.AppendFormat("| {0,-" + maxLength + "} | {1,-" + maxLength2 + "} |\n",
				vars[i] == null ? string.Empty : vars[i].ToString(),
				vars[i + 1] == null ? string.Empty : vars[i + 1].ToString());

		Sb.AppendFormat("{0}\n", new string('-', maxLength + maxLength2 + 7));

		UnityEngine.Debug.Log(Sb.ToString());
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void DT(string message, params object[] vars)
	{
		if (vars.Length / 2 % 1 != 0)
			throw new Exception("Parameters must be key then value. Dividable by two.");

		Sb.Clear();

		var maxLength = 0;
		for (var i = 0; i < vars.Length; i += 2)
			maxLength = Math.Max(maxLength, vars[i] == null ? 0 : vars[i].ToString().Length);

		var maxLength2 = 0;
		for (var i = 1; i < vars.Length; i += 2)
			maxLength2 = Math.Max(maxLength2, vars[i] == null ? 0 : vars[i].ToString().Length);

		Sb.AppendLine(message);

		for (var i = 0; i < vars.Length; i += 2)
			Sb.AppendFormat("| {0,-" + maxLength + "} | {1,-" + maxLength2 + "} |\n",
				vars[i] == null ? string.Empty : vars[i].ToString(),
				vars[i + 1] == null ? string.Empty : vars[i + 1].ToString());

		Sb.AppendFormat("{0}\n", new string('-', maxLength + maxLength2 + 7));

		UnityEngine.Debug.Log(Sb.ToString());
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void D(params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.Log(Sb.ToString());
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void W(params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.LogWarning(Sb.ToString());
	}

	//[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void E(params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.LogError(Sb.ToString());
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void F(params object[] vars)
	{
		var st = new StackTrace();
		var sf = st.GetFrame(1);

		var declaringType = sf?.GetMethod()?.DeclaringType?.ToString() ?? "InvalidType";
		var declaringName = sf?.GetMethod()?.Name ?? "InvalidName";

		Sb.Clear();
		Sb.Append(declaringType);
		Sb.Append(".");
		Sb.Append(declaringName);
		Sb.Append("(");

		for (var i = 0; i < vars.Length; i++)
		{
			Sb.Append(vars[i] == null ? "(null)" : vars[i]);

			if (i < vars.Length - 1)
				Sb.Append(", ");
		}

		Sb.Append(")");

		UnityEngine.Debug.Log(Sb.ToString());
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void OD(UnityEngine.Object obj, params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.Log(Sb.ToString(), obj);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void OW(UnityEngine.Object obj, params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.LogWarning(Sb.ToString(), obj);
	}

	//[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void OE(UnityEngine.Object obj, params object[] vars)
	{
		Sb.Clear();

		foreach (var t in vars)
		{
			Sb.Append(t ?? "(null)");
			Sb.Append(" ");
		}

		UnityEngine.Debug.LogError(Sb.ToString(), obj);
	}
}
