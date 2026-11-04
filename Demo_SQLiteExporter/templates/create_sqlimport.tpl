{@tables
	DROP TABLE IF EXISTS {=table_name:};
	
	{@tableCreate:}    
	
	{@data 
	INSERT INTO {=table_name:} 
	({=fields:}) 
	VALUES ({=values:});
	:}
:}

