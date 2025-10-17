#!/bin/sh
printf '\033c\033]0;%s\a' SoulDiff
base_path="$(dirname "$(realpath "$0")")"
"$base_path/souldiff.x86_64" "$@"
