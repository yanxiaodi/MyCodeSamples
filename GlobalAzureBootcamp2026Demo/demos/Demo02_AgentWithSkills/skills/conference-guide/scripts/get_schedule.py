#!/usr/bin/env python3
"""
get_schedule.py — Conference schedule generator for Global Azure Bootcamp 2026.

Usage:
    python get_schedule.py --track <track-name>

Tracks: AI, Cloud Infrastructure, Developer Tools, Security
If --track is omitted, prints a summary of all tracks.
"""

import argparse
import sys

SCHEDULE = {
    "AI": [
        ("09:00", "09:50", "Getting Started with Microsoft Agent Framework", "Alex Rivera",    "Room A"),
        ("10:00", "10:50", "Azure OpenAI in Production: Lessons Learned",    "Sam Chen",       "Room A"),
        ("11:00", "11:50", "Retrieval-Augmented Generation with AI Search",  "Jordan Lee",     "Room A"),
        ("13:00", "13:50", "Building Multi-Agent Systems on Azure",          "Alex Rivera",    "Room A"),
        ("14:00", "14:50", "Responsible AI: Safety, Fairness and Tracing",   "Morgan Patel",   "Room A"),
        ("15:00", "15:50", "AI-Powered DevOps Pipelines",                   "Casey Williams", "Room A"),
        ("16:00", "16:50", "Semantic Kernel vs Agent Framework: A Deep Dive","Sam Chen",       "Room A"),
    ],
    "Cloud Infrastructure": [
        ("09:00", "09:50", "Landing Zones at Scale with Azure Bicep",        "Jordan Lee",     "Room B"),
        ("10:00", "10:50", "AKS in Production: Cost & Reliability Tips",     "Casey Williams", "Room B"),
        ("11:00", "11:50", "Azure Policy & Governance Automation",           "Morgan Patel",   "Room B"),
        ("13:00", "13:50", "Hybrid Connectivity Deep Dive",                  "Sam Chen",       "Room B"),
        ("14:00", "14:50", "FinOps on Azure: Managing Cloud Spend",          "Jordan Lee",     "Room B"),
    ],
    "Developer Tools": [
        ("09:00", "09:50", "GitHub Copilot for Azure Developers",            "Casey Williams", "Room C"),
        ("10:00", "10:50", ".NET Aspire: Cloud-Native .NET Made Simple",     "Alex Rivera",    "Room C"),
        ("11:00", "11:50", "Azure Developer CLI (azd) in CI/CD",            "Morgan Patel",   "Room C"),
        ("13:00", "13:50", "Dapr on Azure Container Apps",                  "Sam Chen",       "Room C"),
    ],
    "Security": [
        ("09:00", "09:50", "Zero Trust Architecture on Azure",               "Morgan Patel",   "Room D"),
        ("10:00", "10:50", "Microsoft Defender for Cloud: Beyond Basics",    "Casey Williams", "Room D"),
        ("11:00", "11:50", "Entra ID: Modern Identity Patterns",             "Alex Rivera",    "Room D"),
        ("13:00", "13:50", "Secrets Management with Key Vault & RBAC",       "Jordan Lee",     "Room D"),
    ],
}

TRACK_ALIASES = {
    "ai":                    "AI",
    "ml":                    "AI",
    "cloud":                 "Cloud Infrastructure",
    "cloud infrastructure":  "Cloud Infrastructure",
    "infrastructure":        "Cloud Infrastructure",
    "developer":             "Developer Tools",
    "developer tools":       "Developer Tools",
    "devtools":              "Developer Tools",
    "security":              "Security",
    "governance":            "Security",
}


def print_track(track_name: str) -> None:
    canonical = TRACK_ALIASES.get(track_name.lower(), track_name)
    sessions = SCHEDULE.get(canonical)

    if sessions is None:
        available = ", ".join(SCHEDULE.keys())
        print(f"Track '{track_name}' not found. Available tracks: {available}", file=sys.stderr)
        sys.exit(1)

    print(f"\n🗓  {canonical} Track — Global Azure Bootcamp 2026")
    print("=" * 62)
    for start, end, title, speaker, room in sessions:
        print(f"  {start}–{end}  [{room}]")
        print(f"    {title}")
        print(f"    Speaker: {speaker}")
        print()


def print_summary() -> None:
    print("\n🗓  All Tracks — Global Azure Bootcamp 2026")
    print("=" * 62)
    for track, sessions in SCHEDULE.items():
        print(f"\n  📌 {track} ({len(sessions)} sessions)  →  {sessions[0][4]}")
        for start, end, title, speaker, _ in sessions:
            print(f"     {start}–{end}  {title}  [{speaker}]")
    print()


def main() -> None:
    parser = argparse.ArgumentParser(description="GAB2026 Schedule Generator")
    parser.add_argument("--track", type=str, default=None,
                        help="Track name to display (AI, Cloud Infrastructure, Developer Tools, Security)")
    args = parser.parse_args()

    if args.track:
        print_track(args.track)
    else:
        print_summary()


if __name__ == "__main__":
    main()
