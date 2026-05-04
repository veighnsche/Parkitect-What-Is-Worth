using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace WhatIsWorth {
	[HarmonyPatch]
	public class WhatIsWorthPatch_MarketResearchCampaign {
		static MethodBase TargetMethod() => AccessTools.Method(typeof(MarketResearchCampaign), "end");

		[HarmonyPostfix]
		public static void Postfix(MarketResearchCampaign __instance) {
			if (__instance == null || __instance.results == null) {
				return;
			}

			MarketResearchCampaign.ResearchResults results = __instance.results;
			MarketResearchValueEstimateResult estimateResult = MarketResearchValueEstimateAggregator.calculate(results.researchedAt, results.reach);
			MarketResearchValueEstimateCache.store(estimateResult);

			Debug.Log("[WhatIsWorth] Stored Market Research value estimates for " + results.researchedAt);
		}
	}
}
