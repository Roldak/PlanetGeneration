using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public static class Parallel {
	/**
	 * WARNING : UNTESTED CODE
	 */
	public static void For(int from, int to, int step, Action<int> action) {
		if (to <= from || step <= 0) {
			throw new UnityException("Invalid arguments were passed to Parallel.For");
		}

		Thread[] threads = new Thread[(int) Math.Ceiling(((float) to - from) / step)];

		for (int i = 0; i < threads.GetLength(0); ++i) {
			int j = i; // c# why u do dis :'(
			threads[i] = new Thread(delegate () {
				action(j * step + from);
			});
			threads[i].Start();
		}

		foreach (Thread thread in threads) {
			thread.Join();
		}
	}
}
