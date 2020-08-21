#!/bin/bash
pwsh -executionpolicy remotesigned -File $(pwd)/$(dirname $0)/build.ps1