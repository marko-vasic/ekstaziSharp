readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Imports
. "${SCRIPT_DIR}/log_file_paths.sh"

function extract_information() {
    local dir=${1}; shift
    local branch_name=${1}; shift
    (
        cd "${dir}"

        if [ ! -d ".git" ]; then
            for file_name in ${file_names[@]}; do
                local file=$(eval "echo \${${file_name}}")
                local contents=$(cat "${file}")
                echo "${file_name}: \"${contents}\""
            done
        else
            local revisions=""
            if [ "${branch_name}" == "master" ]; then
                revisions=$(git rev-list master --reverse)
            else
                revisions=$(git rev-list "${branch_name}" --reverse --not $(git rev-list master))
            fi
            # make an array
            revisions=($revisions)
            local revisions_count=${#revisions[@]}
            
            for ((i = 0; i < ${#revisions[@]}; i++)); do
                local revision=${revisions[i]}

                git checkout -f "${revision}" &> /dev/null
                local index=$((i + 1))
                # project sha is written in the commit message
                local sha=$(git log -1 --pretty=%B)
                echo "##### REVISION ${index}/${revisions_count} SHA: ${sha} #####"
                
                for file_name in ${file_names[@]}; do
                    local file=$(eval "echo \${${file_name}}")
                    local contents=$(cat "${file}")
                    echo "${file_name}: \"${contents}\""
                done
            done
        fi
    )
}

##### MAIN #####

if [ $# -lt 2 ]; then
    echo "usage:"
    echo "argument 1: path to the logs directory"
    echo "argument 2: output file"
    echo "argument 3 (optional): name of the branch, default is master"
    exit
fi

input_dir=${1}; shift
output_file=${1}; shift
branch_name=${1:-master}; shift

if [ ! -d "${input_dir}/executionLogs" ]; then
    echo "${input_dir} does not contain ekstazi execution logs"
    exit
fi

extract_information "${input_dir}" "${branch_name}" > ${output_file}
