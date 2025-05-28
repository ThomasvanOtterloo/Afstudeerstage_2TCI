import unittest
import csv
import json
from pathlib import Path
from datetime import datetime

from Transformer.BrandIdentifierDecorator import BrandIdentifierDecorator
from Transformer.BaseTransformerModel import BaseTransformerModel


class TestNERModelFromCSV(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        core = BaseTransformerModel()
        cls.model = BrandIdentifierDecorator(core)
        cls.failures = []


    def load_test_cases(self, filepath):
        with open(filepath, encoding="utf-8") as f:
            return list(csv.DictReader(f))

    def test_csv_cases(self):
        csv_path = Path(__file__).parent / "testData.csv"
        test_cases = self.load_test_cases(csv_path)

        total = 0
        failed = 0

        for row in test_cases:
            input_text = row["Input"]
            expected_output = json.loads(row["Output"])

            # Run model
            actual_json = self.model.transformData(json.dumps(input_text))
            actual_output = json.loads(actual_json)

            # Log count mismatch but continue
            if len(actual_output) != len(expected_output):
                print(f"\n⚠️ Object count mismatch for input:\n{input_text}")
                print(f"Expected: {len(expected_output)}, Actual: {len(actual_output)}")
                self.failures.append({
                    "type": "count_mismatch",
                    "input": input_text,
                    "expected_count": len(expected_output),
                    "actual_count": len(actual_output),
                    "actual_output": actual_output
                })
                failed += 1

            for i, (actual, expected) in enumerate(zip(actual_output, expected_output)):
                total += 1
                with self.subTest(input=input_text, index=i):
                    try:
                        if "Brand" in expected:
                            self.assertIn("Brand", actual)
                            self.assertEqual(
                                actual["Brand"].strip().lower(),
                                expected["Brand"].strip().lower()
                            )
                        if "ReferenceNumber" in expected:
                            self.assertIn("ReferenceNumber", actual)
                            self.assertEqual(
                                actual["ReferenceNumber"].strip().lower(),
                                expected["ReferenceNumber"].strip().lower()
                            )
                    except AssertionError as e:
                        failed += 1
                        self.failures.append({
                            "type": "field_mismatch",
                            "input": input_text,
                            "object_index": i,
                            "expected": expected,
                            "actual": actual,
                            "error": str(e)
                        })
                        raise  # Still let unittest record it as failed

        # Save failures to a log file
        if self.failures:
            timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
            log_path = Path(__file__).parent / f"ner_test_failures_{timestamp}.json"
            with open(log_path.with_suffix(".json"), "w", encoding="utf-8") as f:
                json.dump(self.failures, f, indent=2, ensure_ascii=False)

            print(f"\n❌ Logged {len(self.failures)} failures to {log_path.name}")

        passed = total - failed
        pct = round((passed / total) * 100, 2) if total else 0
        print(f"\n✅ Test Summary: {passed}/{total} passed ({pct}%)")


if __name__ == '__main__':
    unittest.main()
