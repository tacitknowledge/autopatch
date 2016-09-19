package com.tacitknowledge.util.migration;

import junit.framework.TestCase;

public class MigrationRunnerFactoryTest extends TestCase
{

	protected void setUp() throws Exception
	{
		super.setUp();
	}

	public void testGetMigrationRunnerStrategy_Default()
	{
		MigrationRunnerStrategy strategy =  MigrationRunnerFactory.getMigrationRunnerStrategy("");
		assertEquals("com.tacitknowledge.util.migration.OrderedMigrationRunnerStrategy",strategy.getClass().getName());
	}

	public void testGetMigrationRunnerStrategy_MissingPatchMigrationRunnerStrategy()
	{
		MigrationRunnerStrategy strategy =  MigrationRunnerFactory.getMigrationRunnerStrategy("com.tacitknowledge.util.migration.MissingPatchMigrationRunnerStrategy");
		assertEquals("com.tacitknowledge.util.migration.MissingPatchMigrationRunnerStrategy",strategy.getClass().getName());
	}

	public void testGetMigrationRunnerStrategy_NoSuchStrategy()
	{
		try
		{
			MigrationRunnerFactory.getMigrationRunnerStrategy("com.tacitknowledge.util.migration.NoSuchStrategy");
			fail("MigrationRunnerFactory.getMigrationRunnerStrategy() Should have raised an IllegalArgumentException");
		}
		catch (IllegalArgumentException e)
		{
			// expected
		}
		
	}
	
	
}
