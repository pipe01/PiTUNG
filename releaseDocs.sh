#!/bin/sh
set -e

export VSINSTALLDIR="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community"
export VisualStudioVersion="15.0"

docfx ./docfx_project/docfx.json

SOURCE_DIR=$PWD
TEMP_REPO_DIR=$PWD/../my-project-gh-pages

echo "Removing temporary doc directory $TEMP_REPO_DIR"
rm -rf $TEMP_REPO_DIR
mkdir $TEMP_REPO_DIR

echo "Cloning the repo with the gh-pages branch"
git clone https://github.com/pipe01/PiTUNG.git --branch gh-pages $TEMP_REPO_DIR 2> /dev/null

echo "Clear repo directory"
cd $TEMP_REPO_DIR
git clean -d -x -n

echo "Copy documentation into the repo"
cp -r $SOURCE_DIR/docfx_project/_site/* .

echo "Push the new docs to the remote branch"
echo "Add"
git add . -A
echo "Commit"
git commit -m "Update generated documentation"
echo "Push"
git push origin gh-pages &> log.txt
echo "Done"
