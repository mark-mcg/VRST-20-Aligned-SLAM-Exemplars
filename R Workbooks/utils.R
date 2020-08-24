check.packages <- function(pkg){
  new.pkg <- pkg[!(pkg %in% installed.packages()[, "Package"])]
  if (length(new.pkg)) 
    install.packages(new.pkg, dependencies = TRUE)
  sapply(pkg, require, character.only = TRUE)
}

plot3D <- function(df, x, y, z){
  plot_ly(df, x = as.formula(paste0("~", x)), y = as.formula(paste0("~", y)), z = as.formula(paste0("~", z)), type = 'scatter3d', mode = 'lines', opacity = 1,
          line = list(width = 6)) %>% layout(scene=list(aspectmode='cube'))
}

dist3d <- function(x1, y1, z1, x2, y2, z2){
  sqrt( (x2-x1)^2 + (y2-y1)^2 + (z2-z1)^2)
}

is.formula <- function(x){
  inherits(x,"formula")
}

generateBoxplot <- function(df, yvar, factorvar, gridformula, grid){
  dodge <- position_dodge(width = 1)
  violinadjust = 1.0
  
  boxplot = ggplot(df, aes_string(x=factorvar, y=yvar))+
    geom_violin(aes_string(colour = factorvar, fill =factorvar ), alpha=0.6, adjust=violinadjust, position = dodge) +
    geom_boxplot(aes_string(colour = factorvar ), width = 0.2, outlier.colour = NULL, outlier.size = 0.6, notch = TRUE, colour="#262626", fill="white", alpha=0.7, position = dodge )
  
  if ( is.formula(gridformula) ){
    if (grid)
      boxplot <- boxplot+facet_grid(gridformula, space = "free", drop=TRUE )
    else
      boxplot <- boxplot+facet_wrap(gridformula, drop=TRUE, scales = "free_x")
  }

  return(boxplot)
}


generateHistogramplot <- function(df, xvar, gridformula, grid){
  plot = ggplot(df, aes_string(x=xvar))+
  geom_histogram()
  
  if ( is.formula(gridformula) ){
    if (grid)
      plot <- plot+facet_grid(gridformula, space = "free", drop=TRUE )
    else
      plot <- plot+facet_wrap(gridformula, drop=TRUE)
  }
  
  return(plot)
}


generateScatterPlot <- function(df, xvar, yvar, limits, gridformula, grid){
  
  plot = ggplot(df, aes_string(x=xvar, y=yvar))+
    stat_density_2d(aes(fill = ..density..), geom = "raster", contour = FALSE) +
    geom_point(alpha = 1/10, size = 1) +
    scale_fill_distiller(palette=4, direction=1) +
    theme(
      legend.position='none'
    )
  
  if (!is.na(limits)){
    plot <- plot + scale_x_continuous(limits = limits) +
    scale_y_continuous(limits = limits)
  }
  
  if ( is.formula(gridformula) ){
    if (grid)
      plot <- plot+facet_grid(gridformula, space = "free", drop=TRUE )
    else
      plot <- plot+facet_wrap(gridformula, drop=TRUE, scales = "free_x")
  }
  return(plot)
}