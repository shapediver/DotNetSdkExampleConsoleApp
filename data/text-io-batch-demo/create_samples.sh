#!/bin/bash

if [ "$#" -lt 2 ]
then
	echo "Usage: $0 NUM_SAMPLES SAMPLE_SIZE"
	exit 1
fi

numsamples=$1
samplesize=$2

for i in $( seq 1 ${numsamples} )
do 
	echo $i
	content=$(openssl rand -hex ${samplesize})
	fn=$(echo $content | cut -b 1-20)
	echo $content > ${fn}.txt
done

