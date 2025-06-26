using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public static class AwaitableExtensions
{
	public static async Awaitable WaitWhile(Func<bool> condition)
	{
		while (condition())
			await Awaitable.NextFrameAsync();
	}

	public static async Awaitable WaitUntil(Func<bool> condition)
	{
		while (!condition())
			await Awaitable.NextFrameAsync();
	}

	public static async Awaitable WaitUntil(Func<bool> condition, CancellationToken cancellationToken)
	{
		while (!condition())
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			await Awaitable.NextFrameAsync(cancellationToken);
		}
	}

	public static async Awaitable WaitFor(IEnumerator enumerator)
	{
		while (enumerator.MoveNext())
			await Awaitable.NextFrameAsync();
	}

	public static void SafeExecute(this Awaitable func)
	{
		_ = SafeExecuteAsync(func);
		return;

		static async Awaitable SafeExecuteAsync(Awaitable func)
		{
			try
			{
				await func;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
