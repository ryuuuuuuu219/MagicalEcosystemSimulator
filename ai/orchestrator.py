import argparse
import json
import subprocess
from datetime import datetime
from pathlib import Path

CODEX = r"C:\Users\sryut\AppData\Roaming\npm\codex.cmd"
BASE_DIR = Path(__file__).resolve().parent
PROMPTS_DIR = BASE_DIR / "prompts"
LOGS_DIR = BASE_DIR / "logs"
LATEST_LOG_PATH = BASE_DIR / "log.json"
HISTORY_LOG_PATH = BASE_DIR / "log_history.jsonl"


def load_prompt(name, **kwargs):
    prompt_path = PROMPTS_DIR / f"{name}.txt"
    template = prompt_path.read_text(encoding="utf-8")
    return template.format(**kwargs)


def run_codex(prompt):
    result = subprocess.run(
        [CODEX, "exec", "--full-auto", prompt],
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    return result.stdout.strip()


def inspector(task):
    prompt = load_prompt("inspector", task=task)
    return run_codex(prompt)


def planner(task, inspection):
    prompt = load_prompt("planner", task=task, inspection=inspection)
    return run_codex(prompt)


def executor(codex_task):
    prompt = load_prompt("coder", codex_task=codex_task)
    return run_codex(prompt)


def risk_review(result):
    prompt = load_prompt("reviewer", result=result)
    return run_codex(prompt)


def save_log(task, inspection, plan, result, review):
    LOGS_DIR.mkdir(exist_ok=True)

    timestamp = datetime.now().astimezone().isoformat(timespec="seconds")
    safe_timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_data = {
        "timestamp": timestamp,
        "task": task,
        "inspection": inspection,
        "plan": plan,
        "result": result,
        "risk_review": review,
    }

    run_log_path = LOGS_DIR / f"{safe_timestamp}.json"
    run_log_path.write_text(
        json.dumps(log_data, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )

    LATEST_LOG_PATH.write_text(
        json.dumps(log_data, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )

    with open(HISTORY_LOG_PATH, "a", encoding="utf-8") as file:
        file.write(json.dumps(log_data, ensure_ascii=False) + "\n")

    return run_log_path


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def print_log_summary(log_data, include_body=True):
    print(f"timestamp: {log_data.get('timestamp', '')}")
    print(f"task: {log_data.get('task', '')}")

    if include_body:
        print("\n=== inspection ===")
        print(log_data.get("inspection", ""))
        print("\n=== plan ===")
        print(log_data.get("plan", ""))
        print("\n=== result ===")
        print(log_data.get("result", ""))
        print("\n=== risk review ===")
        print(log_data.get("risk_review", ""))


def show_last_log():
    if not LATEST_LOG_PATH.exists():
        print("No latest log found.")
        return

    log_data = load_json(LATEST_LOG_PATH)
    print_log_summary(log_data)


def list_logs():
    if not LOGS_DIR.exists():
        print("No logs found.")
        return

    log_paths = sorted(LOGS_DIR.glob("*.json"), reverse=True)
    if not log_paths:
        print("No logs found.")
        return

    for log_path in log_paths:
        log_data = load_json(log_path)
        timestamp = log_data.get("timestamp", "")
        task = log_data.get("task", "")
        print(f"{log_path.name} | {timestamp} | {task}")


def orchestrate(task):
    print("=== inspector ===")
    inspection = inspector(task)
    print(inspection)

    print("\n=== planner ===")
    plan = planner(task, inspection)
    print(plan)

    print("\n=== codex execution ===")
    result = executor(plan)
    print(result)

    print("\n=== risk review ===")
    review = risk_review(result)
    print(review)

    run_log_path = save_log(task, inspection, plan, result, review)
    print(f"\nSaved log: {run_log_path}")


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("task", nargs="?", help="Task to execute")
    parser.add_argument(
        "--last",
        action="store_true",
        help="Show the latest saved log",
    )
    parser.add_argument(
        "--list-logs",
        action="store_true",
        help="List saved run logs",
    )
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    if args.list_logs:
        list_logs()
    elif args.last:
        show_last_log()
    else:
        task = args.task or input("Task: ")
        orchestrate(task)
