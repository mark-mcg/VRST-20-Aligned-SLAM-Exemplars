
#
# Handle loading/installing required packages
#
logUtilsSetup <- function(){
  
  source("utils.R")
  
  # install/load required packages
  packages<-c("kableExtra", "ggplot", "tidyselect", "plot_ly", "doBy", "plyr", "dplyr", "tidyr","tidyverse", "data.table", "lsmeans", "lme4", "stringr", "varhandle", "BayesFactor", "sqldf", 
              "foreach", "iterators", "reticulate", "plyr", "plotly", "peakPick", "jsonlite", "dplyr", "tidyr", "purrr")
  checkresult = check.packages(packages)
}

savePlot <- function(mplot, name, pwidth, pheight){
  if (!dir.exists("generated"))
    dir.create("generated")
  
  ggsave(plot = mplot, file=paste0("generated/plot-", name, ".pdf"), 
         width=pwidth, height=pheight)
  
}

#
# Load all the Unity logs from the given directory, matching the given descriptor/file type.
# If factor column names are provided, these will be set using the Factors variable.
# For large amounts of logs, recommend using first_file_only and working on one log file to begin with
# as processing time can be considerable!
#
loadLogs <- function(startDirectory, log_descriptor, log_file_type, first_file_only = FALSE){
  
  out.file<-""
  directory = paste0(getwd(), startDirectory)
  cat(directory)
  file.names <- dir(directory, pattern = paste0(".*", log_descriptor, ".*", log_file_type), recursive = TRUE)
  total = 0;
  results_df = NULL
  
  details = file.info(file.names)
  details = details[with(details, order(as.POSIXct(mtime))), ]
  files = rownames(details)
  
  file.names = files
  filesToLoad <- if (first_file_only) c(1) else 1:length(file.names)
  
  for(i in filesToLoad){
    filename = file.names[i]
    total = total + 1;
    
    cat(sprintf("Reading file \"%s\" \n", filename))
    
    jsondf = jsonlite:::fromJSON(paste0(directory,filename), simplifyVector = TRUE, flatten = TRUE)[[1]]
    
    #jsondf$ParticipantID = factor(jsondf$ParticipantID)
    jsondf$Logfilename = filename
    
    #cat(sprintf("\"%s\ has %i rows \n", filename, nrow(jsondf)))
    results_df = bind_rows(results_df, jsondf)
    #cat(sprintf("\"%s\ bound successfully \n", filename))
  }
  
  cat(sprintf("Loaded %i \"%s\" files \n", total, log_descriptor))
  
  return(results_df)
}

summaryStats <- function(df, factors){
  summaryBy(factors, FUN=c(mean, median, sd), data=df, na.rm=TRUE)
}