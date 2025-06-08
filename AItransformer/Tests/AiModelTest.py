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
        # Instantiate the core model + decorator once for all tests
        core = BaseTransformerModel()
        cls.model = BrandIdentifierDecorator(core)
        cls.failures = []

    def load_test_cases(self, filepath):
        with open(filepath, encoding="utf-8") as f:
            return list(csv.DictReader(f))

    def test_csv_cases(self):
        """
        For each CSV row: parse the expected JSON list-of-objects,
        run the model, parse its actual JSON list-of-objects,
        then spawn exactly one subTest for each expected object index.
        """
        csv_path = Path(__file__).parent / "testData.csv"
        test_cases = self.load_test_cases(csv_path)

        # Count how many expected objects in total we'll test:
        total_expected_objects = sum(
            len(json.loads(row["Output"]))
            for row in test_cases
        )
        failed_objects = 0     # how many object-level checks fail

        # Now iterate row-by-row
        for row_index, row in enumerate(test_cases):
            input_text = row["Input"]

            # First, parse the expected list-of-objects from the CSV cell
            try:
                expected_output = json.loads(row["Output"])
                if not isinstance(expected_output, list):
                    raise ValueError("Expected JSON is not a list of objects")
            except Exception as e:
                # If the CSV “Output” cell itself is malformed, treat each
                # expected-object as “lost” – but here we just bail out all
                # object-level tests for that row at once.
                self.failures.append({
                    "row_index": row_index,
                    "input": input_text,
                    "stage": "parse_expected",
                    "error": str(e),
                    "raw_expected": row["Output"]
                })
                # We still want to count ALL the objects in expected_output
                # as “failed,” but we didn’t know how many because parsing failed.
                # Simplest: mark the entire row’s objects as missing.
                # If the CSV JSON parser itself failed, we can count 1 failure per row:
                failed_objects += 1
                # Immediately record a failure for *this entire row*, then skip to next row:
                self.fail(f"Row {row_index}: cannot parse expected JSON → {e!r}")
                continue

            # Next, run the model
            try:
                actual_json = self.model.transformData(json.dumps(input_text))
            except Exception as e:
                self.failures.append({
                    "row_index": row_index,
                    "input": input_text,
                    "stage": "transformData_error",
                    "error": str(e)
                })
                # All expected objects for this row become “failed,” but
                # we’ll still explicitly iterate them below to generate subTests.
                # For now just record a failure for each expected object:
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
                # Mark every expected object as “failed” if we couldn’t parse actual JSON
                failed_objects += len(expected_output)
                self.fail(f"Row {row_index}: cannot parse actual JSON → {e!r}")
                continue

            # Now we know expected_output and actual_output are both lists.
            # We want exactly one subTest per index in range(len(expected_output)).
            # If actual_output has fewer items, we’ll catch IndexError and record a “missing” error.
            # If actual_output has extra items, we simply ignore them (we only test indices up to len(expected_output)-1).

            for i in range(len(expected_output)):
                with self.subTest(row_index=row_index, object_index=i):
                    expected_obj = expected_output[i]
                    # If actual_output is shorter than expected, that's a missing-object failure:
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
                        self.fail(f"Row {row_index}, object {i}: expected object is missing in actual output.")
                        # Continue to next object‐subTest
                        continue

                    actual_obj = actual_output[i]
                    object_errors = []

                    # Example field checks (Brand and ReferenceNumber).  Add more as needed.
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

                    # … you can add more field‐by‐field comparisons here exactly as you like …

                    if object_errors:
                        # Record that this single object-subTest failed, with all errors joined
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
                    # If no errors, this subTest “passes” automatically

        # At the end, write out a JSON failure log if there were any fails
        if self.failures:
            timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
            log_path = Path(__file__).parent / f"ner_test_failures_{timestamp}.json"
            with open(log_path.with_suffix(".json"), "w", encoding="utf-8") as f:
                json.dump(self.failures, f, indent=2, ensure_ascii=False)
            print(f"\n❌ Logged {len(self.failures)} object‐level failures to {log_path.name}")

        passed_objects = total_expected_objects - failed_objects
        pct = round((passed_objects / total_expected_objects) * 100, 2) if total_expected_objects else 0
        print(f"\n✅ Test Summary: {passed_objects}/{total_expected_objects} objects passed ({pct}%)")


if __name__ == '__main__':
    unittest.main()
