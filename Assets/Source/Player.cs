class Player
{
	int points = 0;
	
	public int GetPoints()
	{
		return points;
	}
	
	public void AddPoints(int n)
	{
		points += n;
	}
}