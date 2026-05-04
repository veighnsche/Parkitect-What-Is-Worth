using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatIsWorth {
	public static class MarketResearchValueEstimateAggregator {
		public static MarketResearchValueEstimateResult calculate(int researchedAt, float reach) {
			List<Guest> sampledGuests = sampleGuests(reach);
			var result = new MarketResearchValueEstimateResult(researchedAt, reach, sampledGuests.Count);
			result.setAttractionRows(calculateAttractionRows(sampledGuests));
			result.setShopProductRows(calculateShopProductRows(sampledGuests));
			return result;
		}

		private static List<Guest> sampleGuests(float reach) {
			return GameController.Instance.park.getGuests()
				.Where(guest => RandomGenerator.Instance().value > 1f - reach)
				.ToList();
		}

		private static IEnumerable<MarketResearchValueEstimateRow> calculateAttractionRows(List<Guest> sampledGuests) {
			if (sampledGuests.Count == 0) {
				return Enumerable.Empty<MarketResearchValueEstimateRow>();
			}

			var rows = new List<MarketResearchValueEstimateRow>();
			foreach (Attraction attraction in GameController.Instance.park.getAttractions()) {
				if (!isEligibleAttraction(attraction)) {
					continue;
				}

				MarketResearchValueEstimateRow row = calculateRow(
					MarketResearchValueEstimateTargetType.Attraction,
					attraction.GetInstanceID().ToString(),
					attraction.getCustomizedName(),
					attraction.entranceFee,
					sampledGuests,
					guest => attraction.calculateValueFor(guest)
				);

				if (row != null) {
					rows.Add(row);
				}
			}

			return rows;
		}

		private static IEnumerable<MarketResearchValueEstimateRow> calculateShopProductRows(List<Guest> sampledGuests) {
			if (sampledGuests.Count == 0) {
				return Enumerable.Empty<MarketResearchValueEstimateRow>();
			}

			var rows = new List<MarketResearchValueEstimateRow>();
			foreach (Shop shop in GameController.Instance.park.getShops()) {
				var productShop = shop as ProductShop;
				if (productShop == null || !productShop.opened) {
					continue;
				}

				var productShopSettings = productShop.getSettings() as ProductShopSettings;
				if (productShopSettings == null) {
					continue;
				}

				foreach (Product product in productShop.selectedProducts) {
					if (product == null) {
						continue;
					}

					ProductSettings productSettings = productShopSettings.getProductSettings(product);
					if (productSettings == null) {
						continue;
					}

					MarketResearchValueEstimateRow row = calculateRow(
						MarketResearchValueEstimateTargetType.ShopProduct,
						productShop.GetInstanceID() + ":" + product.getReferenceName(),
						productShop.getCustomizedName() + " - " + I18N.GetString(product.getName()),
						productSettings.price,
						sampledGuests,
						guest => product.calculateValueFor(guest)
					);

					if (row != null) {
						rows.Add(row);
					}
				}
			}

			return rows;
		}

		private static bool isEligibleAttraction(Attraction attraction) {
			if (attraction == null) {
				return false;
			}

			return attraction.state == Attraction.State.OPENED && !attraction.isBroken();
		}

		private static MarketResearchValueEstimateRow calculateRow(
			MarketResearchValueEstimateTargetType targetType,
			string targetKey,
			string displayName,
			float currentPrice,
			List<Guest> sampledGuests,
			Func<Person, float> calculateValue
		) {
			var values = new List<float>(sampledGuests.Count);
			var bucketCounts = new Dictionary<float, int>();
			float sum = 0f;

			foreach (Guest guest in sampledGuests) {
				float value = roundValue(calculateValue(guest));
				if (float.IsNaN(value) || float.IsInfinity(value)) {
					continue;
				}

				values.Add(value);
				sum += value;
				bucketCounts.TryGetValue(value, out int count);
				bucketCounts[value] = count + 1;
			}

			if (values.Count == 0) {
				return null;
			}

			values.Sort();
			float average = sum / values.Count;
			float median = calculateMedian(values);
			var buckets = bucketCounts
				.Select(pair => new MarketResearchValueEstimateBucket(pair.Key, pair.Value));

			return new MarketResearchValueEstimateRow(
				targetType,
				targetKey,
				displayName,
				currentPrice,
				values.Count,
				values[0],
				values[values.Count - 1],
				average,
				median,
				buckets
			);
		}

		private static float roundValue(float value) {
			float multiplier = WhatIsWorth._config.multiplier;
			if (multiplier > 0) {
				return UnityEngine.Mathf.Floor(value / multiplier) * multiplier;
			}
			return value;
		}

		private static float calculateMedian(List<float> sortedValues) {
			int middle = sortedValues.Count / 2;
			if (sortedValues.Count % 2 == 1) {
				return sortedValues[middle];
			}
			return (sortedValues[middle - 1] + sortedValues[middle]) / 2f;
		}
	}
}
