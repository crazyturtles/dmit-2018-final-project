<Query Kind="Program">
  <Connection>
    <ID>cfcfa69a-662d-4b4b-aced-a336cf6e0b12</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>(local)</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	Service_GetStandardService().Dump();
}

public List<string> Service_GetStandardService()
	=> StandardJobs.Select(sj => sj.Description).ToList();