import unittest
import csv
import json
import time
from pathlib import Path
from datetime import datetime

from Transformer.BrandIdentifierDecorator import BrandIdentifierDecorator
from Transformer.BaseTransformerModel import BaseTransformerModel


class TestNERModelFromCSV(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        # Instantiate the core model + decorator once for all tests
        core = BaseTransformerModel()
        cls.model = BrandIdentifierDecorator(core)
        cls.failures = []
        cls.timings = []  # will hold per-message durations

    def load_test_cases(self, filepath):
        with open(filepath, encoding="utf-8") as f:
            return list(csv.DictReader(f))

    def test_csv_cases(self):
        """
        For each CSV row: parse the expected JSON list-of-objects,
        run the model (timing it), parse its actual JSON,
        then spawn exactly one subTest for each expected object index.
        """
        csv_path = Path(__file__).parent / "testData.csv"
        test_cases = self.load_test_cases(csv_path)

        # Count how many expected objects in total we'll test:
        total_expected_objects = sum(
            len(json.loads(row["Output"]))
            for row in test_cases
        )
        failed_objects = 0  # how many object-level checks fail

        # Now iterate row-by-row
        for row_index, row in enumerate(test_cases):
            input_text = row["Input"]

            # First, parse the expected list-of-objects from the CSV cell
            try:
                expected_output = json.loads(row["Output"])
                if not isinstance(expected_output, list):
                    raise ValueError("Expected JSON is not a list of objects")
            except Exception as e:
                self.failures.append({
                    "row_index": row_index,
                    "input": input_text,
                    "stage": "parse_expected",
                    "error": str(e),
                    "raw_expected": row["Output"]
                })
                failed_objects += 1
                self.fail(f"Row {row_index}: cannot parse expected JSON → {e!r}")
                continue

            # Run & time the model
            try:
                start = time.perf_counter()
                actual_json = self.model.transformData(json.dumps(input_text))
                elapsed = time.perf_counter() - start
                self.timings.append(elapsed)
            except Exception as e:
                self.failures.append({
                    "row_index": row_index,
                    "input": input_text,
                    "stage": "transformData_error",
                    "error": str(e)
                })
                failed_objects += len(expected_output)
                self.fail(f"Row {row_index}: transformData() raised exception → {e!r}")
                continue

            # Next, parse the actual JSON list-of-objects returned by the model
            try:
                actual_output = json.loads(actual_json)
                if not isinstance(actual_output, list):
                    raise ValueError("Model output JSON is not a list of objects")
            except Exception as e:
                self.failures.append({
                    "row_index": row_index,
                    "input": input_text,
                    "stage": "parse_actual",
                    "error": str(e),
                    "raw_output": actual_json
                })
                failed_objects += len(expected_output)
                self.fail(f"Row {row_index}: cannot parse actual JSON → {e!r}")
                continue

            # Validate each expected object
            for i in range(len(expected_output)):
                with self.subTest(row_index=row_index, object_index=i):
                    expected_obj = expected_output[i]

                    if i >= len(actual_output):
                        failed_objects += 1
                        self.failures.append({
                            "row_index": row_index,
                            "object_index": i,
                            "input": input_text,
                            "stage": "missing_object",
                            "expected": expected_obj,
                            "actual": None,
                        })
                        self.fail(f"Row {row_index}, object {i}: expected object is missing.")
                        continue

                    actual_obj = actual_output[i]
                    object_errors = []

                    # Example field checks
                    if "Brand" in expected_obj:
                        if "Brand" not in actual_obj:
                            object_errors.append(
                                f"Missing key 'Brand' (expected '{expected_obj['Brand']}')"
                            )
                        else:
                            a_val = str(actual_obj["Brand"]).strip().lower()
                            e_val = str(expected_obj["Brand"]).strip().lower()
                            if a_val != e_val:
                                object_errors.append(
                                    f"Brand mismatch: expected '{expected_obj['Brand']}', got '{actual_obj['Brand']}'"
                                )

                    if "ReferenceNumber" in expected_obj:
                        if "ReferenceNumber" not in actual_obj:
                            object_errors.append(
                                f"Missing key 'ReferenceNumber' (expected '{expected_obj['ReferenceNumber']}')"
                            )
                        else:
                            a_val = str(actual_obj["ReferenceNumber"]).strip().lower()
                            e_val = str(expected_obj["ReferenceNumber"]).strip().lower()
                            if a_val != e_val:
                                object_errors.append(
                                    f"ReferenceNumber mismatch: expected '{expected_obj['ReferenceNumber']}', got '{actual_obj['ReferenceNumber']}'"
                                )

                    # You can add more field-by-field comparisons here...

                    if object_errors:
                        failed_objects += 1
                        self.failures.append({
                            "row_index": row_index,
                            "object_index": i,
                            "input": input_text,
                            "stage": "field_mismatch",
                            "errors": object_errors,
                            "expected": expected_obj,
                            "actual": actual_obj,
                        })
                        combined = "\n  - " + "\n  - ".join(object_errors)
                        self.fail(f"Row {row_index}, object {i} failed:\n  -{combined}")

        # At the end, write out a JSON failure log if there were any failures
        if self.failures:
            timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
            log_path = Path(__file__).parent / f"ner_test_failures_{timestamp}.json"
            with open(log_path, "w", encoding="utf-8") as f:
                json.dump(self.failures, f, indent=2, ensure_ascii=False)
            print(f"\n❌ Logged {len(self.failures)} object-level failures to {log_path.name}")

        # Print performance summary
        if self.timings:
            total_time = sum(self.timings)
            per_msg_avg = total_time / len(self.timings)
            per_obj_avg = total_time / total_expected_objects
            print(
                f"\n⏱ Performance: {len(self.timings)} messages in {total_time:.2f}s "
                f"(avg {per_msg_avg:.4f}s/msg, {per_obj_avg:.4f}s/object)"
            )

        passed_objects = total_expected_objects - failed_objects
        pct = round((passed_objects / total_expected_objects) * 100, 2) if total_expected_objects else 0
        print(f"\n✅ Test Summary: {passed_objects}/{total_expected_objects} objects passed ({pct}%)")

    @classmethod
    def tearDownClass(cls):
        # Optionally, you could dump timings or additional artifacts here
        pass


if __name__ == '__main__':
    unittest.main()
