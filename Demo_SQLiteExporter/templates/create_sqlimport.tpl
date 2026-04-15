{@tables
	DROP TABLE IF EXISTS {=table_name:};
	
	{@tableCreate:}    
	
	INSERT INTO {=table_name:} 
	{@data ({=fields:}) 
	VALUES ({=values:});:}
:}

