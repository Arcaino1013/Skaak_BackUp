#!/bin/bash

# Set the maximum number of files per commit
MAX_FILES_PER_COMMIT=8

# Function to traverse directories recursively
traverse_directory() {
    local dir="$1"
    local file_count=0
    local commit_number=1

    for file in "$dir"/*; do
        if [ -d "$file" ]; then
            traverse_directory "$file"
        else
            echo "Looping"
            file_status=$(git status --porcelain "$file" | cut -c 1,2)
            if [ "$file_status" == "M" ] || [ "$file_status" == "D" ] || [ "$file_status" == "??" ]; then
                ((file_count++))
                echo "Files added to staging area: $file_count $file"
                if [ "$file_count" -gt "$MAX_FILES_PER_COMMIT" ]; then
                    echo "Create new commit"
                    git commit -m "Commit $commit_number"
                    git push origin HEAD
                    ((commit_number++))
                    file_count=1
                    sleep 30
                fi
                git add "$file"
            fi
        fi
    done
}

# Start traversing the current directory recursively
traverse_directory "."

# Final commit for remaining files
git commit -m "Final commit"

read -p "Press Enter to exit"


