#!/usr/bin/env bash
if [! -d ~/.rvm ]
then
    apt-get install -y curl
    curl -sSL https://get.rvm.io | bash -s $1
fi 
