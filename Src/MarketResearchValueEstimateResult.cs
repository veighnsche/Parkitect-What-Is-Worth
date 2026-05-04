using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatIsWorth {
	public enum MarketResearchValueEstimateTargetType {
		Attraction,
		ShopProduct
	}

	public class MarketResearchValueEstimateBucket {
		public float value;
		public int count;

		public MarketResearchValueEstimateBucket(float value, int count) {
			this.value = value;
			this.count = count;
		}
	}

	public class MarketResearchValueEstimateRow {
		public MarketResearchValueEstimateTargetType targetType;
		public string targetKey;
		public string displayName;
		public float currentPrice;
		public int sampleCount;
		public float minValue;
		public float maxValue;
		public float averageValue;
		public float medianValue;
		public float pricingGap;
		public List<MarketResearchValueEstimateBucket> buckets = new List<MarketResearchValueEstimateBucket>();

		public MarketResearchValueEstimateRow(
			MarketResearchValueEstimateTargetType targetType,
			string targetKey,
			string displayName,
			float currentPrice,
			int sampleCount,
			float minValue,
			float maxValue,
			float averageValue,
			float medianValue,
			IEnumerable<MarketResearchValueEstimateBucket> buckets
		) {
			this.targetType = targetType;
			this.targetKey = targetKey;
			this.displayName = displayName;
			this.currentPrice = currentPrice;
			this.sampleCount = sampleCount;
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.averageValue = averageValue;
			this.medianValue = medianValue;
			this.pricingGap = Math.Abs(currentPrice - averageValue);
			this.buckets = buckets.OrderBy(bucket => bucket.value).ToList();
		}

		public List<MarketResearchValueEstimateBucket> getDisplayBuckets(int maxBucketCount, int maxBucketsBeforeTruncation) {
			if (buckets.Count <= maxBucketsBeforeTruncation) {
				return new List<MarketResearchValueEstimateBucket>(buckets);
			}

			return buckets
				.OrderByDescending(bucket => bucket.count)
				.ThenBy(bucket => bucket.value)
				.Take(maxBucketCount)
				.OrderBy(bucket => bucket.value)
				.ToList();
		}
	}

	public class MarketResearchValueEstimateResult {
		public const int DISPLAY_ROW_LIMIT = 20;
		public const int DISPLAY_BUCKET_LIMIT = 5;
		public const int DISPLAY_BUCKET_TRUNCATION_THRESHOLD = 6;

		public int researchedAt;
		public float reach;
		public int sampleCount;
		public List<MarketResearchValueEstimateRow> attractionRows = new List<MarketResearchValueEstimateRow>();
		public List<MarketResearchValueEstimateRow> shopProductRows = new List<MarketResearchValueEstimateRow>();

		public MarketResearchValueEstimateResult(int researchedAt, float reach, int sampleCount) {
			this.researchedAt = researchedAt;
			this.reach = reach;
			this.sampleCount = sampleCount;
		}

		public void setAttractionRows(IEnumerable<MarketResearchValueEstimateRow> rows) {
			attractionRows = getRowsForDisplay(rows);
		}

		public void setShopProductRows(IEnumerable<MarketResearchValueEstimateRow> rows) {
			shopProductRows = getRowsForDisplay(rows);
		}

		private static List<MarketResearchValueEstimateRow> getRowsForDisplay(IEnumerable<MarketResearchValueEstimateRow> rows) {
			return rows
				.OrderByDescending(row => row.pricingGap)
				.ThenBy(row => row.displayName)
				.Take(DISPLAY_ROW_LIMIT)
				.ToList();
		}
	}

	public static class MarketResearchValueEstimateCache {
		private static readonly Dictionary<int, MarketResearchValueEstimateResult> resultsByResearchTime = new Dictionary<int, MarketResearchValueEstimateResult>();

		public static void store(MarketResearchValueEstimateResult result) {
			resultsByResearchTime[result.researchedAt] = result;
		}

		public static bool tryGet(int researchedAt, out MarketResearchValueEstimateResult result) {
			return resultsByResearchTime.TryGetValue(researchedAt, out result);
		}

		public static void clear() {
			resultsByResearchTime.Clear();
		}
	}
}
